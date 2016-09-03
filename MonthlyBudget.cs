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
        private double lastUpdate;
        private int budgetInterval;
        private int friendlyInterval = 30;
        private int multiplier = 2227;
        private int availableWages = 5000;
        private int assignedWages = 10000;
        private int vesselCost = 10000;
        private bool hardMode;
        private readonly string savedFile = KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/MonthlyBudgetData.cfg";


        private void Budget(double timeSinceLastUpdate)
        {
            try
            {
                double funds = Funding.Instance.Funds;
                double costs = 0;
                double offsetFunds = funds;
                if (budgetInterval*2 > timeSinceLastUpdate)
                {
                    if (hardMode)
                    {
                        int penalty = (int) funds/10000;
                        Reputation.Instance.AddReputation(-penalty, TransactionReasons.None);
                        Debug.Log("MonthlyBudgets: " + funds + "remaining, " + penalty + " reputation removed");
                    }
                    costs = CostCalculate();
                    offsetFunds = funds - costs;
                }
                float rep = Reputation.CurrentRep;
                double budget = (rep * multiplier) - costs;
                    //we shouldn't take money away. If the player holds more than the budget, just don't award.
                    if (budget <= offsetFunds)
                    {
                        Funding.Instance.AddFunds(-costs, TransactionReasons.None);
                        ScreenMessages.PostScreenMessage("We can't justify extending your budget this month");
                        if (costs > 0)
                        {
                            ScreenMessages.PostScreenMessage("This month's costs total " + costs.ToString("C"));
                        }
                        Debug.Log("MonthlyBudgets: Budget of " + budget + " is less than available funds of " + funds);
                    }

                    else
                    {
                        Funding.Instance.AddFunds(-funds, TransactionReasons.None);
                        Funding.Instance.AddFunds(budget, TransactionReasons.None);
                        ScreenMessages.PostScreenMessage("This month's budget is " + budget.ToString("C"));
                        Debug.Log("MonthlyBudgets: Budget awarded: " + budget);
                    }
                    lastUpdate = lastUpdate + budgetInterval;
            }
            catch
            {
                Debug.Log("MonthlyBudgets: Problem calculating the budget");
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
            GameEvents.onGameStateSaved.Add(OnGameStateSaved);
            GameEvents.onGameStateLoad.Add(OnGameStateLoad);

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
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER) return;
            double time = (Planetarium.GetUniversalTime());
            while (lastUpdate > time)
            {
                lastUpdate = lastUpdate - budgetInterval;
                Debug.Log("MonthlyBudgets: Last update was in the future. Using time machine to correct");
            }
            double timeSinceLastUpdate = time - lastUpdate;
            if (timeSinceLastUpdate >= budgetInterval)
            {
                Budget(timeSinceLastUpdate);
            }
        }

        void OnDestroy()
        {
            GameEvents.onGameStateSaved.Remove(OnGameStateSaved);
            GameEvents.onGameStateLoad.Remove(OnGameStateLoad);
        }

        private int CostCalculate()
        {
            IEnumerable<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew;
            int availableBudget = crew.Count(a => a.rosterStatus == ProtoCrewMember.RosterStatus.Available)*availableWages;
            int assignedBudget = crew.Count(a => a.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)* assignedWages;
            IEnumerable<Vessel> vessels = FlightGlobals.Vessels.Where(v => v.vesselType != VesselType.Debris);
            int vesselBudget = vessels.Count() *vesselCost;
            int budget = availableBudget + assignedBudget + vesselBudget;
            Debug.Log("MonthlyBudgets: Expenses are " + budget);
            return budget;
        }

        private void OnGameStateLoad(ConfigNode ignore)
        {
            if (File.Exists(savedFile))
            {
                ConfigNode node = ConfigNode.Load(savedFile);
                if (node != null)
                {
                    double.TryParse(node.GetValue("TimeElapsed (DO NOT CHANGE)"), out lastUpdate);
                    int.TryParse(node.GetValue("Multiplier"), out multiplier);
                    int.TryParse(node.GetValue("Budget Interval (Kerbin Days)"), out friendlyInterval);
                    int.TryParse(node.GetValue("Unassigned Kerbals wage"), out availableWages);
                    int.TryParse(node.GetValue("Assigned Kerbals wage"), out assignedWages);
                    int.TryParse(node.GetValue("Base Vessel Cost"), out vesselCost);
                    bool.TryParse(node.GetValue("Hard Mode"), out hardMode);
                    Debug.Log("MonthlyBudgets: Loaded data");
                }
            }
            else
            {
                Debug.Log("MonthlyBudgets: No existing data found for this save");
            }
            budgetInterval = friendlyInterval * 60 * 60 * 6;
            Debug.Log("MonthlyBudgets: Set Interval to " + budgetInterval +" (from "+friendlyInterval +" days)");
            }

        private void OnGameStateSaved(Game ignore)
        {
            if (HighLogic.LoadedSceneIsEditor) return;
            ConfigNode savedNode = new ConfigNode();
            savedNode.AddValue("TimeElapsed (DO NOT CHANGE)", lastUpdate);
            savedNode.AddValue("Multiplier", multiplier);
            savedNode.AddValue("Budget Interval (Kerbin Days)", friendlyInterval);
            savedNode.AddValue("Unassigned Kerbals wage", availableWages);
            savedNode.AddValue("Assigned Kerbals wage", assignedWages);
            savedNode.AddValue("Base Vessel Cost", vesselCost);
            savedNode.AddValue("Hard Mode", hardMode);
            savedNode.Save(savedFile);
            Debug.Log("MonthlyBudgets: Saved data");
        }
    }
}
