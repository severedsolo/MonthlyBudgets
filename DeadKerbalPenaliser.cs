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
            }
        }

        public void OnDestroy()
        {
            GameEvents.onCrewKilled.Remove(onCrewKilled);
        }
        public void onCrewKilled(EventReport evtdata)
        {
            Reputation.Instance.AddReputation(-100, TransactionReasons.None);
        }
    }
}
