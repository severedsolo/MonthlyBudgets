using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KSP.UI.Screens;
using System;
using MonthlyBudgets_KACWrapper;
using Experience;
namespace severedsolo
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class MonthlyBudgets : MonoBehaviour
    {
        public static double lastUpdate = 99999;
        private float budgetInterval;
        private float friendlyInterval = 30;
        private int multiplier = 2227;
        private int availableWages = 1000;
        private int assignedWages = 2000;
        private int vesselCost = 10000;
        private bool hardMode;
        private bool RepDecayEnabled;
        private bool coverCosts;
        bool showGUI = false;
        ApplicationLauncherButton ToolbarButton;
        Rect Window = new Rect(20, 100, 240, 50);
        float loanPercentage = 1.0f;
        float RepDecay = 0.1f;
        bool timeDiscrepancyLog = true;
        bool stopTimeWarp;

        private void Budget(double timeSinceLastUpdate)
        {
            try
            {
                double funds = Funding.Instance.Funds;
                float costs = 0;
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
                    ScreenMessages.PostScreenMessage("We can't justify extending your budget this month");
                    if (budget < costs || !coverCosts)
                    {
                        if (costs > 0)
                        {
                            Funding.Instance.AddFunds(-costs, TransactionReasons.None);
                            ScreenMessages.PostScreenMessage("This month's costs total " + costs.ToString("C"));
                        }
                    }
                    else
                    {
                        ScreenMessages.PostScreenMessage("The budget will cover your costs");
                        float repLoss = costs / multiplier;
                        Reputation.Instance.AddReputation(-repLoss, TransactionReasons.None);
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
                if (RepDecayEnabled)
                {
                    if(RepDecay>=1)RepDecay = RepDecay / 100;
                    Reputation.Instance.AddReputation(-Reputation.CurrentRep*(RepDecay), TransactionReasons.None);
                    Debug.Log("[MonthlyBudgets]: Removing " + RepDecay + "% Reputation");
                }
                if(!KACWrapper.AssemblyExists && stopTimeWarp)
                {
                    TimeWarp.SetRate(0, true);
                }
            }
            catch
            {
                Debug.Log("[MonthlyBudgets]: Problem calculating the budget");
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
            if (!HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().masterSwitch) Destroy(this);
            GameEvents.onGameStateSave.Add(OnGameStateSave);
            GameEvents.onGameStateLoad.Add(OnGameStateLoad);
            GameEvents.onGUIApplicationLauncherReady.Add(GUIReady);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);

        }
        void Start()
        {
            KACWrapper.InitKACWrapper();
            Debug.Log(KACWrapper.KAC.Alarms.Count);
        }

        void Update()
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER) return;
            if (lastUpdate == 99999) return;
            double time = (Planetarium.GetUniversalTime());
            while (lastUpdate > time)
            {
                lastUpdate = lastUpdate - budgetInterval;
                if (timeDiscrepancyLog)
                {
                    Debug.Log("[MonthlyBudgets]: Last update was in the future. Using time machine to correct");
                    timeDiscrepancyLog = false;
                }
            }
            double timeSinceLastUpdate = time - lastUpdate;
            if (timeSinceLastUpdate >= budgetInterval)
            {
                Budget(timeSinceLastUpdate);
            }
            if (KACWrapper.AssemblyExists && stopTimeWarp)
            {
                if (!KACWrapper.APIReady) return;
                KACWrapper.KACAPI.KACAlarmList alarms = KACWrapper.KAC.Alarms;
                if (alarms.Count == 0) return;
                for (int i = 0; i < alarms.Count; i++)
                {
                    string s = alarms[i].Name;
                    if (s == "Next Budget") return;
                }
                KACWrapper.KAC.CreateAlarm(KACWrapper.KACAPI.AlarmTypeEnum.Raw, "Next Budget", lastUpdate + budgetInterval);
            }
        }

        void OnDestroy()
        {
            GameEvents.onGameStateSave.Remove(OnGameStateSave);
            GameEvents.onGameStateLoad.Remove(OnGameStateLoad);
            GameEvents.onGUIApplicationLauncherReady.Remove(GUIReady);
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
            GameEvents.onGameSceneSwitchRequested.Remove(onGameSceneSwitchRequested);
        }

        private int CostCalculate(bool log)
        {
            IEnumerable<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew;
            int budget = 0;
            foreach (ProtoCrewMember p in crew)
            {
                if (p.type == ProtoCrewMember.KerbalType.Tourist) continue;
                float level = p.experienceLevel;
                if (level == 0) level = 0.5f;
                float wages = 0;
                if (p.rosterStatus == ProtoCrewMember.RosterStatus.Available) wages = level * availableWages;
                if (p.rosterStatus == ProtoCrewMember.RosterStatus.Assigned) wages = level * assignedWages;
                budget = budget + (int)wages;
            }
            IEnumerable<Vessel> vessels = FlightGlobals.Vessels.Where(v => v.vesselType != VesselType.Debris && v.vesselType != VesselType.Flag && v.vesselType != VesselType.SpaceObject && v.vesselType != VesselType.Unknown && v.vesselType != VesselType.EVA);
            int vesselBudget = vessels.Count() * vesselCost;
            budget = budget + vesselBudget;
            if (log)
            {
                Debug.Log("[MonthlyBudgets]: Expenses are " + budget);
            }
            return budget;
        }

        private void OnGameStateLoad(ConfigNode node)
        {
            if (!float.TryParse(node.GetValue("EmergencyFunding"), out loanPercentage)) loanPercentage = 1.0f;
            multiplier = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().Multiplier;
            friendlyInterval = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().friendlyInterval;
            availableWages = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().availableWages;
            assignedWages = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().assignedWages;
            hardMode = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().HardMode;
            budgetInterval = friendlyInterval * 60 * 60 * 6;
            RepDecayEnabled = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().DecayEnabled;
            RepDecay = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().RepDecay;
            vesselCost = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().vesselCost;
            if (!double.TryParse(node.GetValue("LastBudgetUpdate"), out lastUpdate)) lastUpdate = budgetInterval * 1000;
            timeDiscrepancyLog = true;
            coverCosts = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().coverCosts;
            stopTimeWarp = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().stopTimewarp;
            Debug.Log("[MonthlyBudgets]: Set Interval to " + budgetInterval + " (from " + friendlyInterval + " days)");
        }

        private void OnGameStateSave(ConfigNode savedNode)
        {
            savedNode.AddValue("LastBudgetUpdate", lastUpdate);
            savedNode.AddValue("EmergencyFunding", loanPercentage);
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
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER || HighLogic.LoadedScene == GameScenes.MAINMENU) return;
            if (ToolbarButton == null)
            {
                ToolbarButton = ApplicationLauncher.Instance.AddModApplication(GUISwitch, GUISwitch, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, GameDatabase.Instance.GetTexture("MonthlyBudgets/Icon", false));
            }
        }

        void GUIDisplay(int windowID)
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
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
            int RepLoss = (int)Reputation.CurrentRep / 10;
            if (loanAmount > 0)
            {
                if (GUILayout.Button("Apply for Emergency Funding ("+loanAmount+")"))
                {
                    Reputation.Instance.AddReputation(-RepLoss, TransactionReasons.None);
                    Funding.Instance.AddFunds(loanAmount, TransactionReasons.None);
                    loanPercentage = loanPercentage - 0.1f;
                    Debug.Log("[MonthlyBudgets]: Emergency Funding Awarded");
                }
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
            multiplier = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().Multiplier;
            friendlyInterval = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().friendlyInterval;
            availableWages = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().availableWages;
            assignedWages = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().assignedWages;
            hardMode = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().HardMode;
            budgetInterval = friendlyInterval * 60 * 60 * 6;
            RepDecayEnabled = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().DecayEnabled;
            vesselCost = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().vesselCost;
            RepDecay = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().RepDecay / 100;
            coverCosts = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().coverCosts;
            stopTimeWarp = HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().stopTimewarp;
        }
        void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            if (ToolbarButton == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(ToolbarButton);
            showGUI = false;
        }
    }
}