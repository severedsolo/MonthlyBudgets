using UnityEngine;

namespace severedsolo
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class DeadKerbalPenaliser : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(this);
            GameEvents.onCrewKilled.Add(onCrewKilled);
        }

        void Start()
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
            {
                Destroy(this);
                Debug.Log("MonthlyBudgets: Game is not Career - Monthly Budgets will stop");
            }
        }
        public void OnDestroy()
        {
            GameEvents.onCrewKilled.Remove(onCrewKilled);
        }
        public void onCrewKilled(EventReport evtdata)
        {
            Reputation.Instance.AddReputation(-50, TransactionReasons.None);
            Debug.Log(evtdata +" died. 100 reputation removed");
        }
    }
}
