using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using KSP.UI.Screens;
using System;

namespace severedsolo
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class MonthlyBudgets : MonoBehaviour
    {
        public static double lastUpdate;
        private float budgetInterval;
        private float friendlyInterval = 30;
        private int multiplier = 2227;
        private int availableWages = 5000;
        private int assignedWages = 10000;
        private int vesselCost = 10000;
        private bool hardMode;
        private readonly string savedFile = KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/MonthlyBudgetData.dat";
        bool showGUI = false;
        ApplicationLauncherButton ToolbarButton;
        Rect Window = new Rect(20, 100, 240, 50);
        float loanPercentage = 1;


        private void Budget(double timeSinceLastUpdate)
        {
            try
            {
                double funds = Funding.Instance.Funds;
                double costs = 0;
                double offsetFunds = funds;
                if (budgetInterval * 2 > timeSinceLastUpdate)
                {
                    if (hardMode)
                    {
                        int penalty = (int)funds / 10000;
                        Reputation.Instance.AddReputation(-penalty, TransactionReasons.None);
                        Debug.Log("[MonthlyBudgets]: " + funds + "remaining, " + penalty + " reputation removed");
                    }
                    costs = CostCalculate(true);
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
                    Debug.Log("[MonthlyBudgets]: Budget of " + budget + " is less than available funds of " + funds);
                }

                else
                {
                    Funding.Instance.AddFunds(-funds, TransactionReasons.None);
                    Funding.Instance.AddFunds(budget, TransactionReasons.None);
                    ScreenMessages.PostScreenMessage("This month's budget is " + budget.ToString("C"));
                    Debug.Log("[MonthlyBudgets]: Budget awarded: " + budget);
                }
                lastUpdate = lastUpdate + budgetInterval;
                if (loanPercentage < 1) loanPercentage = loanPercentage + 0.1f;
            }
            catch
            {
                Debug.Log("[MonthlyBudgets]: Problem calculating the budget");
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
            GameEvents.onGameStateSaved.Add(OnGameStateSaved);
            GameEvents.onGameStateLoad.Add(OnGameStateLoad);
            GameEvents.onGUIApplicationLauncherReady.Add(GUIReady);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);

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
                Debug.Log("[MonthlyBudgets]: Last update was in the future. Using time machine to correct");
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
            GameEvents.onGUIApplicationLauncherReady.Remove(GUIReady);
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
            Destroy(ToolbarButton);
        }

        private int CostCalculate(bool log)
        {
            IEnumerable<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew;
            int availableBudget = crew.Count(a => a.rosterStatus == ProtoCrewMember.RosterStatus.Available) * availableWages;
            int assignedBudget = crew.Count(a => a.rosterStatus == ProtoCrewMember.RosterStatus.Assigned) * assignedWages;
            IEnumerable<Vessel> vessels = FlightGlobals.Vessels.Where(v => v.vesselType != VesselType.Debris && v.vesselType != VesselType.Flag && v.vesselType != VesselType.SpaceObject && v.vesselType != VesselType.Unknown && v.vesselType != VesselType.EVA);
            int vesselBudget = vessels.Count() * vesselCost;
            int budget = availableBudget + assignedBudget + vesselBudget;
            if (log)
            {
                Debug.Log("[MonthlyBudgets]: Expenses are " + budget);
            }
            return budget;
        }

        private void OnGameStateLoad(ConfigNode ignore)
        {
            if (File.Exists(savedFile))
            {
                ConfigNode node = ConfigNode.Load(savedFile);
                double.TryParse(node.GetValue("TimeElapsed (DO NOT CHANGE)"), out lastUpdate);
                float.TryParse(node.GetValue("Emergency Funding"), out loanPercentage);
            }
            multiplier = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().Multiplier;
            friendlyInterval = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().friendlyInterval;
            availableWages = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().availableWages;
            assignedWages = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().assignedWages;
            hardMode = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().HardMode;
            budgetInterval = friendlyInterval * 60 * 60 * 6;
            Debug.Log("[MonthlyBudgets]: Set Interval to " + budgetInterval + " (from " + friendlyInterval + " days)");
        }

        private void OnGameStateSaved(Game ignore)
        {
            if (HighLogic.LoadedSceneIsEditor) return;
            ConfigNode savedNode = new ConfigNode();
            savedNode.AddValue("TimeElapsed (DO NOT CHANGE)", lastUpdate);
            savedNode.AddValue("EmergencyFunding", loanPercentage);
            savedNode.Save(savedFile);
            Debug.Log("[MonthlyBudgets]: Saved data");
        }

        public void OnGUI()
        {
            if (showGUI)
            {

               Window = GUILayout.Window(65468754, Window, GUIDisplay, "MonthlyBudgets", GUILayout.Width(200));
            }
        }
        public void GUIReady()
        {
            if (ToolbarButton == null)
            {
                ToolbarButton = ApplicationLauncher.Instance.AddModApplication(GUISwitch, GUISwitch, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, GameDatabase.Instance.GetTexture("MonthlyBudgets/Icon", false));
            }
        }

        void GUIDisplay(int windowID)
        {
        if(HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
            {
                GUILayout.Label("MonthlyBudgets is only available in Career Games");
                return;
            }
         int costs = CostCalculate(false);
         int estimatedBudget = (int)Reputation.CurrentRep * multiplier;
            if(estimatedBudget <0)
            {
                estimatedBudget = 0;
            }
         double nextUpdateRaw = lastUpdate + budgetInterval;
         float nextUpdateRefine = (float)nextUpdateRaw/6/60/60;
         int year = 1;
         int day = 1;
         while (nextUpdateRefine > 426.08)
         {
                year = year + 1;
                nextUpdateRefine = nextUpdateRefine - 426.08f;
         }
            day = day + (int)nextUpdateRefine;
            GUILayout.Label("Next Budget Due: Y " + year + " D " + day);
            GUILayout.Label("Estimated Budget: $" + estimatedBudget);
            GUILayout.Label("Current Costs: $" + costs);
            double loanAmount = Math.Round(((Reputation.CurrentRep*multiplier)/10) * loanPercentage, 0);
            if (loanAmount <= 0) return;
            if(GUILayout.Button("Borrow "+loanAmount +" Funds"))
            {
                Reputation.Instance.AddReputation(-Reputation.CurrentRep/10, TransactionReasons.None);
                Funding.Instance.AddFunds(loanAmount, TransactionReasons.None);
                loanPercentage = loanPercentage - 0.1f;
            }
            GUI.DragWindow();

        }


        public void GUISwitch()
        {
            if (showGUI)
            {
                showGUI = false;
            }
            else
            {
                showGUI = true;
            }
        }
        void OnGameSettingsApplied()
        {
            OnGameStateLoad(new ConfigNode(null, null));
        }
    }
}