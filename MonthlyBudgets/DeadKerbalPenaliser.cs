using System;
using UnityEngine;

namespace MonthlyBudgets
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class DeadKerbalPenaliser : MonoBehaviour
    {
        Guid lastProcessedVessel = Guid.Empty;
        public void Awake()
        {
            if (!BudgetSettings.instance.masterSwitch) Destroy(this);
            DontDestroyOnLoad(this);
            GameEvents.onKerbalStatusChange.Add(OnKerbalStatusChange);
        }

        private void OnKerbalStatusChange(ProtoCrewMember p, ProtoCrewMember.RosterStatus statusFrom, ProtoCrewMember.RosterStatus statusTo)
        {
            if (statusTo != ProtoCrewMember.RosterStatus.Dead) return;
            Debug.Log("[MonthlyBudgets]: "+p.name+" changed status from "+statusFrom+"to "+statusTo);
            int penalty = (int)(Reputation.CurrentRep * ((float)BudgetSettings.instance.kerbalDeathPenalty/100));
            Reputation.Instance.AddReputation(-penalty, TransactionReasons.VesselLoss);
            Debug.Log("[MonthlyBudgets] " + p.name + " aboard " + p.seat.vessel.vesselName +
                      " has died. Applying penalty of "+penalty+" rep");
            if (lastProcessedVessel == p.seat.vessel.id) return;
            penalty = (int)(Reputation.CurrentRep * ((float)BudgetSettings.instance.vesselDeathPenalty/100));
            Reputation.Instance.AddReputation(-penalty, TransactionReasons.VesselLoss);
            Debug.Log("[MonthlyBudgets] "+ p.seat.vessel.vesselName +
                      " First Loss. Applying penalty of "+penalty+" rep");
            lastProcessedVessel = p.seat.vessel.id;
        }
    }
}
