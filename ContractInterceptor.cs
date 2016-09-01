using UnityEngine;
using Contracts;

namespace severedsolo
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class ContractInterceptor : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(this);
            GameEvents.Contract.onOffered.Add(onOffered);
        }

        void Start()
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
            {
                Destroy(this);
            }
        }

        public void OnDestroy()
        {
            GameEvents.Contract.onOffered.Remove(onOffered);
        }

        public void onOffered(Contract contract)
        {
            if (contract.FundsCompletion > 0)
            {
                int rep = (int)((contract.FundsAdvance / 10000) + (contract.FundsCompletion / 10000));
                contract.ReputationCompletion = contract.ReputationCompletion + rep;
                contract.FundsAdvance = 0;
                contract.FundsCompletion = 0;
                Debug.Log("MonthlyBudgets: Intercepted " + contract.ToString() + " and removed fund award. An extra " + rep + " reputation will be awarded instead");
            }
        }
    }
}