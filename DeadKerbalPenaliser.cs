using UnityEngine;

namespace MonthlyBudgets
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class DeadKerbalPenaliser : MonoBehaviour
    {
        public void Awake()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().masterSwitch) Destroy(this);
            DontDestroyOnLoad(this);
            GameEvents.onCrewKilled.Add(onCrewKilled);
        }

        public void OnDestroy()
        {
            GameEvents.onCrewKilled.Remove(onCrewKilled);
        }
        private void onCrewKilled(EventReport evtdata)
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER) return;
            int penalty = (int)Reputation.CurrentRep / 4;
            Reputation.Instance.AddReputation(-penalty, TransactionReasons.None);
            Debug.Log("[MonthlyBudgets]: A Kerbal has died. " +penalty +" reputation removed");
        }
    }
}
