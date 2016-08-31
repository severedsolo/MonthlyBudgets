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
            GameEvents.Contract.onParameterChange.Add(onParameterChange);
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
            int rep = (int)((contract.FundsAdvance / 10000) + (contract.FundsCompletion / 10000));
            contract.ReputationCompletion = contract.ReputationCompletion + rep;
            contract.FundsAdvance = 0;
            contract.FundsCompletion = 0;
            Debug.Log("MonthlyBudgets: Intercepted " +contract.ToString() +" funds set to 0. An extra "+rep +" reputation has been awarded");
        }

        public void onParameterChange (Contract contract, ContractParameter parameter)
        {
            Funding.Instance.AddFunds(-parameter.FundsCompletion, TransactionReasons.ContractReward);
        }
    }
}