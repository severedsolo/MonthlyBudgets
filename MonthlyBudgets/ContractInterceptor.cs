using System.Collections.Generic;
using System.Linq;
using Contracts;
using UnityEngine;

namespace MonthlyBudgets
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    internal class ContractInterceptor : MonoBehaviour
    {
        private bool _disableContracts = true;
        private readonly List<string> _disabledAgents = new List<string>();

        public void Awake()
        {
            if (!BudgetSettings.instance.masterSwitch) Destroy(this);
            DontDestroyOnLoad(this);
            GameEvents.Contract.onOffered.Add(OnOffered);
            GameEvents.OnGameSettingsApplied.Add(OnSettings);
            GameEvents.onGameStateLoad.Add(OnLoaded);
            if (_disableContracts) Debug.Log("[MonthlyBudgets]: Starting Contract Interceptor");
            if (!_disableContracts) Debug.Log("[MonthlyBudgets]: Contract Interceptor has been disabled");
        }

        private void Start()
        {
            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("MB_DISABLED_AGENTS");
            for (int i = 0; i < nodes.Length; i++)
            {
                ConfigNode cn = nodes.ElementAt(i);
                string s = cn.GetValue("Agent");
                _disabledAgents.Add(s);
                Debug.Log("[MonthlyBudgets]: Added " + s + " to the agent blacklist");
            }
        }

        private void OnLoaded(ConfigNode data)
        {
            _disableContracts = BudgetSettings.instance.contractInterceptor;
            if (!_disableContracts) Debug.Log("[MonthlyBudgets]: Contract Interceptor has been disabled");
        }

        private void OnSettings()
        {
            _disableContracts = BudgetSettings.instance.contractInterceptor;
            if (_disableContracts) Debug.Log("[MonthlyBudgets]: Starting Contract Interceptor");
            if (!_disableContracts) Debug.Log("[MonthlyBudgets]: Contract Interceptor has been disabled");
        }

        public void OnDestroy()
        {
            GameEvents.Contract.onOffered.Remove(OnOffered);
        }

        private void OnOffered(Contract contract)
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER || !_disableContracts) return;
            if (contract.FundsCompletion <= 0) return;
            if (_disabledAgents.Contains(contract.Agent.Name))
            {
                Debug.Log("[MonthlyBudgets]: " + contract.Title + " has disabled agent " + contract.Agent.Name +
                          " - skipping intercept");
                return;
            }

            float rep = (float) contract.FundsAdvance / 10000 * -1 - (float) contract.FundsFailure / 10000;
            contract.FundsFailure = 0;
            contract.ReputationFailure = rep - contract.ReputationFailure;
            rep = (float) contract.FundsAdvance / 10000 + (float) contract.FundsCompletion / 10000;
            for (int i = 0; i < contract.AllParameters.Count(); i++)
            {
                ContractParameter p = contract.AllParameters.ElementAt(i);
                rep += (float) p.FundsCompletion / 10000;
                p.FundsCompletion = 0;
            }

            contract.ReputationCompletion += rep;
            if (contract.ReputationCompletion < 1) contract.ReputationCompletion = 1;
            contract.FundsAdvance = 0;
            contract.FundsCompletion = 0;
            Debug.Log("[MonthlyBudgets]: Intercepted " + contract.ContractID + "of type " + contract.Title +
                      ": Removed fund award. An extra " + rep + " reputation will be awarded instead");
        }
    }
}