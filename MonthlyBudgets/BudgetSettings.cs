using System;
using UnityEngine;
using System.IO;

namespace MonthlyBudgets
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class BudgetSettings : MonoBehaviour
    {
        public static BudgetSettings instance;
        public bool masterSwitch = true;
        public bool hardMode = false;
        public bool decayEnabled = false;
        public bool contractInterceptor = true;
        public bool coverCosts = false;
        public bool stopTimewarp = true;
        public int repDecay = 10;
        public int multiplier = 2227;
        public float friendlyInterval = 30;
        public int availableWages = 1000;
        public int assignedWages = 2000;
        public int vesselCost = 5000;
        public bool firstRun = true;
        public bool showGUI = false;
        public bool buildingCostsEnabled = true;
        public int buildingCosts = 428;
        public bool launchCostsEnabled = true;
        public int launchCostsVAB = 1000;
        public int launchCostsSPH = 100;

        Rect Window = new Rect(100, 200, 240, 50);
        string SavedFile = KSPUtil.ApplicationRootPath + "/GameData/MonthlyBudgets/PluginData/MonthlyBudgetDefaults.cfg";
        void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        public void OnGUI()
        {
            if (showGUI)
            {
                Window = GUILayout.Window(20989359, Window, GUIDisplay, "MonthlyBudgets Settings", GUILayout.Width(200));
            }
        }

        private void GUIDisplay(int id)
        {
            masterSwitch = GUILayout.Toggle(masterSwitch, "Mod Enabled");
            if (masterSwitch)
            {
                hardMode = GUILayout.Toggle(hardMode, "Deduct reputation for not spending entire budget?");
                contractInterceptor = GUILayout.Toggle(contractInterceptor, "Contracts pay reputation instead of funds?");
                coverCosts = GUILayout.Toggle(coverCosts, "Always try to deduct costs from budget, even if current funds are higher?");
                stopTimewarp = GUILayout.Toggle(stopTimewarp, "Stop Timewarp/Set KAC Alarms on Budget");
                decayEnabled = GUILayout.Toggle(decayEnabled, "Decay Reputation each budget?");
                if (decayEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Decay percentage");
                    int.TryParse(GUILayout.TextField(repDecay.ToString()), out repDecay);
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label("Budget Interval");
                float.TryParse(GUILayout.TextField(friendlyInterval.ToString()), out friendlyInterval);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Budget Multiplier");
                int.TryParse(GUILayout.TextField(multiplier.ToString()), out multiplier);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Unassigned Kerbal Wages (at experience level 1)");
                int.TryParse(GUILayout.TextField(availableWages.ToString()), out availableWages);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Assigned Kerbal Wages (at experience level 1");
                int.TryParse(GUILayout.TextField(assignedWages.ToString()), out assignedWages);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Monthly cost per active vessel");
                int.TryParse(GUILayout.TextField(vesselCost.ToString()), out vesselCost);
                GUILayout.EndHorizontal();
                buildingCostsEnabled = GUILayout.Toggle(buildingCostsEnabled, "Enable maintenance costs for buildings?");
                if (buildingCostsEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Monthly Cost Per Building at Lv 1:");
                    int.TryParse(GUILayout.TextField(buildingCosts.ToString()), out buildingCosts);
                    GUILayout.EndHorizontal();
                }
                launchCostsEnabled = GUILayout.Toggle(launchCostsEnabled, "Enable maintenance costs for launches?");
                if (launchCostsEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Cost Per Launch (VAB):");
                    int.TryParse(GUILayout.TextField(launchCostsVAB.ToString()), out launchCostsVAB);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Cost Per Launch (SPH):");
                    int.TryParse(GUILayout.TextField(launchCostsSPH.ToString()), out launchCostsSPH);
                    GUILayout.EndHorizontal();
                }
            }
            if (GUILayout.Button("Close"))
            {
                showGUI = false;
                firstRun = false;
            }
            GUI.DragWindow();
        }

        public void FirstRun()
        {
            firstRun = false;
            showGUI = true;
            if (!File.Exists(SavedFile)) return;
            ConfigNode settings = ConfigNode.Load(SavedFile);
            bool.TryParse(settings.GetValue("masterSwitch"), out masterSwitch);
            bool.TryParse(settings.GetValue("hardMode"), out hardMode);
            bool.TryParse(settings.GetValue("contractInterceptor"), out contractInterceptor);
            bool.TryParse(settings.GetValue("coverCosts"), out coverCosts);
            bool.TryParse(settings.GetValue("stopTimewarp"), out stopTimewarp);
            bool.TryParse(settings.GetValue("decayEnabled"), out decayEnabled);
            int.TryParse(settings.GetValue("repDecay"), out repDecay);
            int.TryParse(settings.GetValue("multiplier"), out multiplier);
            int.TryParse(settings.GetValue("availableWages"), out availableWages);
            int.TryParse(settings.GetValue("assignedWages"), out assignedWages);
            int.TryParse(settings.GetValue("vesselCost"), out vesselCost);
            bool.TryParse(settings.GetValue("buildingCostsEnabled"), out buildingCostsEnabled);
            int.TryParse(settings.GetValue("buildingCosts"), out buildingCosts);
            bool.TryParse(settings.GetValue("launchCostsEnabled"), out launchCostsEnabled);
            int.TryParse(settings.GetValue("launchCostsVAB"), out launchCostsVAB);
            int.TryParse(settings.GetValue("launchCostsSPH"), out launchCostsSPH);
        }
    }
}
