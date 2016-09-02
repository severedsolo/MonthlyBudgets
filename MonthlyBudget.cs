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
        private double LastUpdate = 0;
        private int BudgetInterval;
        private int friendlyInterval = 30;
        private int multiplier = 2227;
        private int AvailableWages = 5000;
        private int AssignedWages = 10000;
        private int VesselCost = 10000;
        bool hardMode = false;
        string SavedFile = KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/MonthlyBudgetData.cfg";


        private void Budget(double time)
        {
            try
            {
                if (LastUpdate > time)
                {
                    LastUpdate = LastUpdate - BudgetInterval;
                    Debug.Log("MonthlyBudgets: Last update was in the future. Using time machine to correct");
                }
                if ((time - LastUpdate) >= BudgetInterval)
                {
                    double funds = Funding.Instance.Funds;
                    if(hardMode == true)
                    {
                        int penalty = (int)funds / 10000;
                        Reputation.Instance.AddReputation(-penalty, TransactionReasons.None);
                        Debug.Log("MonthlyBudgets: " + funds + "remaining, " + penalty + " reputation removed");
                    }
                    double costs = CostCalculate();
                    double offsetFunds = funds - costs;
                    float rep = Reputation.CurrentRep;
                    double budget = (rep * multiplier) - costs;
                    //we shouldn't take money away. If the player holds more than the budget, just don't award.
                    if (budget <= offsetFunds)
                    {
                        Funding.Instance.AddFunds(-costs, TransactionReasons.None);
                        ScreenMessages.PostScreenMessage("We can't justify extending your budget this month");
                        ScreenMessages.PostScreenMessage("This month's costs total " + costs.ToString("C"));
                        Debug.Log("MonthlyBudgets: Budget of " + budget + " is less than available funds of " + funds);
                    }

                    else
                    {
                        Funding.Instance.AddFunds(-funds, TransactionReasons.None);
                        Funding.Instance.AddFunds(budget, TransactionReasons.None);
                        ScreenMessages.PostScreenMessage("This month's budget is " + budget.ToString("C"));
                        Debug.Log("MonthlyBudgets: Budget awarded: " + budget);
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
                double time = (Planetarium.GetUniversalTime());
                if ((time - LastUpdate) >= BudgetInterval)
                {
                    Budget(time);
                }
            }
        }

        void OnDestroy()
        {
            GameEvents.onGameStateSaved.Remove(onGameStateSaved);
            GameEvents.onGameStateLoad.Remove(onGameStateLoad);
        }

        public int CostCalculate()
        {
            int Budget = 0;
            IEnumerable<ProtoCrewMember> AvailableCrew = HighLogic.CurrentGame.CrewRoster.Crew.Where(k => k.rosterStatus == ProtoCrewMember.RosterStatus.Available);
            IEnumerable<ProtoCrewMember> AssignedCrew = HighLogic.CurrentGame.CrewRoster.Crew.Where(k => k.rosterStatus == ProtoCrewMember.RosterStatus.Assigned);
            int AvailableBudget = 0;
            int AssignedBudget = 0;
            if (AvailableCrew != null)
            {
                AvailableBudget = AvailableCrew.Count() * AvailableWages;
            }

            if (AssignedCrew != null)
            {
                 AssignedBudget = AssignedCrew.Count() * AssignedWages;
            }
            List<Vessel> vessels = FlightGlobals.Vessels.ToList();
            int VesselBudget = (vessels.Count()) * VesselCost;
            Budget = AvailableBudget + AssignedBudget + VesselBudget;
            Debug.Log("MonthlyBudgets: Expenses are " + Budget);
            return Budget;
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
                        bool.TryParse(node.GetValue("Hard Mode"), out hardMode);
                        Debug.Log("MonthlyBudgets: Loaded data");
                    }
                    else
                    {
                        Debug.Log("MonthlyBudgets: No existing data found for this save");
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
                savedNode.AddValue("Hard Mode", hardMode);
                savedNode.Save(SavedFile);
                Debug.Log("MonthlyBudgets: Saved data");
            }
        }
    }
}
