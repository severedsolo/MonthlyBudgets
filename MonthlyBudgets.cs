using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KSP.UI.Screens;
using System;
using MonthlyBudgets_KACWrapper;
using Experience;

namespace MonthlyBudgets
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class MonthlyBudgets : MonoBehaviour
    {
        public static MonthlyBudgets instance;
        public double lastUpdate = 0;
        public float emergencyBudgetPercentage = 10;
        public bool enableEmergencyBudget;
        public double emergencyBudget = 0;
        bool showGUI = false;
        ApplicationLauncherButton ToolbarButton;
        Rect Window = new Rect(20, 100, 240, 50);
        bool timeDiscrepancyLog = true;
        CelestialBody HomeWorld;
        double dayLength;
        double yearLength;
        public string inputString;

        private void Budget(double timeSinceLastUpdate)
        {
            try
            {
                double funds = Funding.Instance.Funds;
                float costs = 0;
                double offsetFunds = funds;
                if ((HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().friendlyInterval*dayLength) * 2 > timeSinceLastUpdate)
                {
                    if (HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().HardMode)
                    {
                        int penalty = (int)funds / 10000;
                        Reputation.Instance.AddReputation(-penalty, TransactionReasons.None);
                        Debug.Log("[MonthlyBudgets]: " + funds + "remaining, " + penalty + " reputation removed");
                    }
                    costs = CostCalculate(true);
                    offsetFunds = funds - costs;
                }
                float rep = Reputation.CurrentRep;
                double budget = (rep * HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().Multiplier) - costs;
                //we shouldn't take money away. If the player holds more than the budget, just don't award.
                if (budget <= offsetFunds)
                {
                    ScreenMessages.PostScreenMessage("We can't justify extending your budget this month");
                    if (budget < costs || !HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().coverCosts)
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
                        float repLoss = costs / HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().Multiplier;
                        Reputation.Instance.AddReputation(-repLoss, TransactionReasons.None);
                    }
                    Debug.Log("[MonthlyBudgets]: Budget of " + budget + " is less than available funds of " + funds);
                }
                else
                {
                    if (enableEmergencyBudget)
                    {
                        double upgradeBudgetReserved = budget * (emergencyBudgetPercentage / 100);
                        budget = budget - upgradeBudgetReserved;
                        emergencyBudget = emergencyBudget + upgradeBudgetReserved;
                        emergencyBudget = Math.Round(emergencyBudget, 0);
                        Debug.Log("[MonthlyBudgets]: Diverted " + emergencyBudgetPercentage + "% of budget. BPF is now: "+emergencyBudget);
                    }
                    Funding.Instance.AddFunds(-funds, TransactionReasons.None);
                    Funding.Instance.AddFunds(budget, TransactionReasons.None);
                    ScreenMessages.PostScreenMessage("This month's budget is " + budget.ToString("C"));
                    Debug.Log("[MonthlyBudgets]: Budget awarded: " + budget);
                }
                lastUpdate = lastUpdate + (HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().friendlyInterval*dayLength);
                if (HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().DecayEnabled)
                {
                    Reputation.Instance.AddReputation(-Reputation.CurrentRep*(HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().RepDecay/100), TransactionReasons.None);
                    Debug.Log("[MonthlyBudgets]: Removing " + HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().RepDecay / 100 + "% Reputation");
                }
                if(!KACWrapper.AssemblyExists && HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().stopTimewarp)
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
            instance = this;
            GameEvents.onGUIApplicationLauncherReady.Add(GUIReady);
            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);

        }
        void Start()
        {
            KACWrapper.InitKACWrapper();
            PopulateHomeWorldData();
        }

        void Update()
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER) return;
            if (lastUpdate == 99999) return;
            if (emergencyBudgetPercentage < 1) emergencyBudgetPercentage = 10;
            if (emergencyBudgetPercentage > 50) emergencyBudgetPercentage = 50;
            double time = (Planetarium.GetUniversalTime());
            while (lastUpdate > time)
            {
                lastUpdate = lastUpdate - (HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().friendlyInterval * dayLength);
                if (timeDiscrepancyLog)
                {
                    Debug.Log("[MonthlyBudgets]: Last update was in the future. Using time machine to correct");
                    timeDiscrepancyLog = false;
                }
            }
            double timeSinceLastUpdate = time - lastUpdate;
            if (timeSinceLastUpdate >= (HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().friendlyInterval * dayLength))
            {
                Budget(timeSinceLastUpdate);
            }
            if (KACWrapper.AssemblyExists && HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().stopTimewarp)
            {
                if (!KACWrapper.APIReady) return;
                KACWrapper.KACAPI.KACAlarmList alarms = KACWrapper.KAC.Alarms;
                if (alarms.Count == 0) return;
                for (int i = 0; i < alarms.Count; i++)
                {
                    string s = alarms[i].Name;
                    if (s == "Next Budget") return;
                }
                KACWrapper.KAC.CreateAlarm(KACWrapper.KACAPI.AlarmTypeEnum.Raw, "Next Budget", lastUpdate + (HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().friendlyInterval * dayLength));
            }
        }

        void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(GUIReady);
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
                if (p.rosterStatus == ProtoCrewMember.RosterStatus.Available) wages = level * HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().availableWages;
                if (p.rosterStatus == ProtoCrewMember.RosterStatus.Assigned) wages = level * HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().assignedWages;
                budget = budget + (int)wages;
            }
            IEnumerable<Vessel> vessels = FlightGlobals.Vessels.Where(v => v.vesselType != VesselType.Debris && v.vesselType != VesselType.Flag && v.vesselType != VesselType.SpaceObject && v.vesselType != VesselType.Unknown && v.vesselType != VesselType.EVA);
            int vesselBudget = vessels.Count() * HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().vesselCost;
            budget = budget + vesselBudget;
            if (log)
            {
                Debug.Log("[MonthlyBudgets]: Expenses are " + budget);
            }
            return budget;
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

        void PopulateHomeWorldData()
        {
            HomeWorld = FlightGlobals.GetHomeBody();
            dayLength = HomeWorld.solarDayLength;
            yearLength = HomeWorld.orbit.period;
        }

        void GUIDisplay(int windowID)
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
            {
                GUILayout.Label("MonthlyBudgets is only available in Career Games");
                return;
            }
            if (HomeWorld == null) PopulateHomeWorldData();
            int costs = CostCalculate(false);
            int estimatedBudget = (int)Reputation.CurrentRep * HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().Multiplier;
            if (estimatedBudget < 0)
            {
                estimatedBudget = 0;
            }
            double nextUpdateRaw = lastUpdate + (HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().friendlyInterval * dayLength);
            double nextUpdateRefine = nextUpdateRaw / dayLength;
            int year = 1;
            int day = 1;
            while (nextUpdateRefine > yearLength / dayLength)
            {
                year = year + 1;
                nextUpdateRefine = nextUpdateRefine - (yearLength / dayLength);
            }
            day = day + (int)nextUpdateRefine;
            GUILayout.Label("Next Budget Due: Y " + year + " D " + day);
            GUILayout.Label("Estimated Budget: $" + estimatedBudget);
            GUILayout.Label("Current Costs: $" + costs);
            enableEmergencyBudget = GUILayout.Toggle(enableEmergencyBudget, "Enable Big Project Fund");
            GUILayout.Label("Big Project Fund: $" + emergencyBudget);
            GUILayout.Label("Percentage to divert to fund");
            float.TryParse(GUILayout.TextField(emergencyBudgetPercentage.ToString()), out emergencyBudgetPercentage);
            if (GUILayout.Button("Withdraw Funds from Big Project Fund"))
            {
                Funding.Instance.AddFunds(emergencyBudget, TransactionReasons.Strategies);
                emergencyBudget = 0;
                enableEmergencyBudget = false;
            }
            GUI.DragWindow();
        }

        public void GUISwitch()
        {
            showGUI = !showGUI;
        }

        void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            if (ToolbarButton == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(ToolbarButton);
            showGUI = false;
        }
    }
}