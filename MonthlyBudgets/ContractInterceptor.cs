using UnityEngine;
using Contracts;
using System.Linq;
using System.Collections.Generic;

namespace MonthlyBudgets
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class ContractInterceptor : MonoBehaviour
    {
        bool disableContracts = true;
        List<string> disabledAgents = new List<string>();

        public void Awake()
        {
            if (!BudgetSettings.instance.masterSwitch) Destroy(this);
            DontDestroyOnLoad(this);
            GameEvents.Contract.onOffered.Add(onOffered);
            GameEvents.OnGameSettingsApplied.Add(onSettings);
            GameEvents.onGameStateLoad.Add(onLoaded);
            if (disableContracts) Debug.Log("[MonthlyBudgets]: Starting Contract Interceptor");
            if (!disableContracts) Debug.Log("[MonthlyBudgets]: Contract Interceptor has been disabled");
        }

        void Start()
        {
            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("MB_DISABLED_AGENTS");
            for(int i = 0; i<nodes.Count(); i++)
            {
                ConfigNode cn = nodes.ElementAt(i);
                string s = cn.GetValue("Agent");
                disabledAgents.Add(s);
                Debug.Log("[MonthlyBudgets]: Added " + s + " to the agent blacklist");
            }
        }

        private void onLoaded(ConfigNode data)
        {
            disableContracts = BudgetSettings.instance.contractInterceptor;
            if (!disableContracts) Debug.Log("[MonthlyBudgets]: Contract Interceptor has been disabled");
        }

        private void onSettings()
        {
            disableContracts = BudgetSettings.instance.contractInterceptor;
            if(disableContracts)Debug.Log("[MonthlyBudgets]: Starting Contract Interceptor");
            if(!disableContracts)Debug.Log("[MonthlyBudgets]: Contract Interceptor has been disabled");
        }

        public void OnDestroy()
        {
            GameEvents.Contract.onOffered.Remove(onOffered);
        }

        private void onOffered(Contract contract)
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER || !disableContracts) return;
            if (contract.FundsCompletion <= 0) return;
            if (disabledAgents.Contains(contract.Agent.Name))
            {
                Debug.Log("[MonthlyBudgets]: " + contract.Title + " has disabled agent " + contract.Agent.Name + " - skipping intercept");
                return;
            }
            float rep = ((float)contract.FundsAdvance / 10000 * -1) - ((float)contract.FundsFailure / 10000);
            contract.FundsFailure = 0;
            contract.ReputationFailure = rep - contract.ReputationFailure;
            rep = ((float)contract.FundsAdvance / 10000) + ((float)contract.FundsCompletion / 10000);
            for (int i = 0; i < contract.AllParameters.Count(); i++)
            {
                ContractParameter p = contract.AllParameters.ElementAt(i);
                rep = rep + ((float)p.FundsCompletion / 10000);
                p.FundsCompletion = 0;
            }

            contract.ReputationCompletion = contract.ReputationCompletion + rep;
            if (contract.ReputationCompletion < 1) contract.ReputationCompletion = 1;
            contract.FundsAdvance = 0;
            contract.FundsCompletion = 0;
            Debug.Log("[MonthlyBudgets]: Intercepted " + contract.ContractID + "of type " +contract.Title+ ": Removed fund award. An extra " + rep + " reputation will be awarded instead");
        }
    }
}