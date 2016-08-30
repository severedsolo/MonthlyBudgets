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
            GameEvents.Contract.onAccepted.Add(onAccepted);
            GameEvents.Contract.onCompleted.Add(onCompleted);
            GameEvents.Contract.onFailed.Add(onFailed);
            GameEvents.Contract.onCancelled.Add(onCancelled);
            GameEvents.Contract.onParameterChange.Add(onParameterChange);
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
            GameEvents.Contract.onAccepted.Remove(onAccepted);
            GameEvents.Contract.onCompleted.Remove(onCompleted);
            GameEvents.Contract.onFailed.Remove(onFailed);
            GameEvents.Contract.onCancelled.Remove(onCancelled);
            GameEvents.Contract.onParameterChange.Remove(onParameterChange);
        }

        public void onAccepted(Contract contract)
        {
            Funding.Instance.AddFunds(-contract.FundsAdvance, TransactionReasons.ContractAdvance);
        }

        public void onCompleted(Contract contract)
        {
            Funding.Instance.AddFunds(-contract.FundsCompletion, TransactionReasons.ContractReward);
        }

        public void onFailed(Contract contract)
        {
            Funding.Instance.AddFunds(-contract.FundsFailure, TransactionReasons.ContractPenalty);
        }

        public void onCancelled(Contract contract)
        {
            Funding.Instance.AddFunds(-contract.FundsFailure, TransactionReasons.ContractPenalty);
        }

        public void onParameterChange (Contract contract, ContractParameter parameter)
        {
            Funding.Instance.AddFunds(-parameter.FundsCompletion, TransactionReasons.ContractReward);
        }
    }
}