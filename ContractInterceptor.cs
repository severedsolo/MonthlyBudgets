using UnityEngine;
using Contracts;
using System;

namespace severedsolo
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class ContractInterceptor : MonoBehaviour
    {
        bool disableContracts = true;
        public void Awake()
        {
            DontDestroyOnLoad(this);
            GameEvents.Contract.onOffered.Add(onOffered);
            GameEvents.OnGameSettingsApplied.Add(onSettings);
            GameEvents.onGameStateLoad.Add(onLoaded);
        }

        private void onLoaded(ConfigNode data)
        {
            disableContracts = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().ContractInterceptor;
        }

        private void onSettings()
        {
            disableContracts = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().ContractInterceptor;
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
            if (!(contract.FundsCompletion > 0)) return;
            int rep = (int)((contract.FundsAdvance / 10000) + (contract.FundsCompletion / 10000));
            contract.ReputationCompletion = contract.ReputationCompletion + rep;
            contract.FundsAdvance = 0;
            contract.FundsCompletion = 0;
            Debug.Log("[MonthlyBudgets]: Intercepted " + contract.ContractID + "of type " +contract.Title+ ": Removed fund award. An extra " + rep + " reputation will be awarded instead");
        }
    }
}