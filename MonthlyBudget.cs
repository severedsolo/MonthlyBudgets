using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System;

namespace severedsolo
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class MonthlyBudgets : MonoBehaviour
    {
        private bool debug = false;
        private double LastUpdate = 0;
        private int BudgetInterval;
        private int friendlyInterval = 30;
        private float Rep = 0.0f;
        private double budget = 0.0f;
        private double funds = 0.0f;
        private int multiplier = 500;
        private int AvailableWages = 5000;
        private int AssignedWages = 10000;
        private int VesselCost = 10000;
        string SavedFile = KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/MonthlyBudgetData.cfg";


        private void Budget()
        {
            try
            {
                double time = (Planetarium.GetUniversalTime());
                if ((time - LastUpdate) >= BudgetInterval)
                {
                    
                    Rep = Reputation.CurrentRep;
                    funds = Funding.Instance.Funds;
                    double costs = CrewBudget() - LogisticBudget();
                    double offsetFunds = funds - costs;
                    budget = (Rep * multiplier) - costs;
                    if (budget <= offsetFunds)
                    {
                        Funding.Instance.AddFunds(-costs, TransactionReasons.None);
                        if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                        {
                            ScreenMessages.PostScreenMessage("We can't justify extending your budget this month");
                            ScreenMessages.PostScreenMessage("This month's costs total " + costs.ToString("C"));
                        }

                    }
                    else
                    {
                        Funding.Instance.AddFunds(-funds, TransactionReasons.None);
                        Funding.Instance.AddFunds(budget, TransactionReasons.None);
                        if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                        {
                            ScreenMessages.PostScreenMessage("This month's budget is " + budget.ToString("C"));
                        }
                    }
                    LastUpdate = LastUpdate + BudgetInterval;
                }
            }
            catch
            {
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
            GameEvents.onGameStateSaved.Add(onGameStateSaved);
            GameEvents.onGameStateLoad.Add(onGameStateLoad);

        }

        void Start()
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
            {
                Destroy(this);
            }
        }

        void Update()
        {
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                Budget();
            }
        }

        void OnDestroy()
        {
            GameEvents.onGameStateSaved.Remove(onGameStateSaved);
            GameEvents.onGameStateLoad.Remove(onGameStateLoad);
        }

        public int CrewBudget()
        {
            int Budget = 0;
            IEnumerable<ProtoCrewMember> AvailableCrew = HighLogic.CurrentGame.CrewRoster.Crew.Where(k => k.rosterStatus == ProtoCrewMember.RosterStatus.Available);
            IEnumerable<ProtoCrewMember> AssignedCrew = HighLogic.CurrentGame.CrewRoster.Crew.Where(k => k.rosterStatus == ProtoCrewMember.RosterStatus.Assigned);

            int CrewCount = 0;
            int AvailableBudget = 0;
            int AssignedBudget = 0;
            if (AvailableCrew != null)
            {

                foreach (ProtoCrewMember crew in AvailableCrew)
                {
                    CrewCount = CrewCount + 1;
                }
                AvailableBudget = CrewCount * AvailableWages;
            }
            CrewCount = 0;
            if (AssignedCrew != null)
            {
                foreach (ProtoCrewMember crew in AssignedCrew)
                {
                    CrewCount = CrewCount + 1;
                }
                AssignedBudget = CrewCount * AssignedWages;
            }
            Budget = AvailableBudget + AssignedBudget;
            Debug.Log("MonthlyBudgets: Crew Budget is " + Budget);
            return Budget;
        }
        public int LogisticBudget()
        {
            List<Vessel> vessels = FlightGlobals.Vessels.ToList();
            int VesselCount = 0;
            if (vessels == null)
            {
                return 0;
            }
            foreach (Vessel v in vessels)
            {
                int i = v.GetCrewCount();
                if (i > 0)
                {
                    VesselCount = VesselCount + 2;
                }
                else
                {
                    VesselCount = VesselCount + 1;
                }
            }
            return VesselCount * VesselCost;
        }

        public void onGameStateLoad(ConfigNode ignore)
        {
                if (File.Exists(SavedFile))
                {

                    ConfigNode node = ConfigNode.Load(SavedFile);
                    if (node != null)
                    {
                        double.TryParse(node.GetValue("TimeElapsed (DO NOT CHANGE)"), out LastUpdate);
                        int.TryParse(node.GetValue("Multiplier"), out multiplier);
                        int.TryParse(node.GetValue("Budget Interval (Kerbin Days)"), out friendlyInterval);
                        int.TryParse(node.GetValue("Unassigned Kerbals wage"), out AvailableWages);
                        int.TryParse(node.GetValue("Assigned Kerbals wage"), out AssignedWages);
                        int.TryParse(node.GetValue("Base Vessel Cost"), out VesselCost);
                    }
                    else
                    {
                        Debug.Log("MonthlyBudgets: No save data found (this message is harmless, as long as this isn't an existing career");
                    }
                }
                BudgetInterval = friendlyInterval * 60 * 60 * 6;
            Debug.Log("MonthlyBudgets: Set Interval to " + BudgetInterval +" (from "+friendlyInterval +" days)");
            }

        public void onGameStateSaved(Game ignore)
        {
            if (!HighLogic.LoadedSceneIsEditor)
            {
                ConfigNode savedNode = new ConfigNode();
                savedNode.AddValue("TimeElapsed (DO NOT CHANGE)", LastUpdate);
                savedNode.AddValue("Multiplier", multiplier);
                savedNode.AddValue("Budget Interval (Kerbin Days)", friendlyInterval);
                savedNode.AddValue("Unassigned Kerbals wage", AvailableWages);
                savedNode.AddValue("Assigned Kerbals wage", AssignedWages);
                savedNode.AddValue("Base Vessel Cost", VesselCost);
                savedNode.Save(SavedFile);
                Debug.Log("MonthlyBudgets: Saved game");
            }
        }
    }
}
