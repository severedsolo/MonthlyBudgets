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
        public int vesselCost = 10000;
        public bool firstRun = true;
        public bool showGUI = false;
        Rect Window = new Rect(20, 100, 240, 50);
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
                GUILayout.Label("Decay percentage");
                if (decayEnabled) int.TryParse(GUILayout.TextField(repDecay.ToString()), out repDecay);
                GUILayout.Label("Budget Multiplier");
                int.TryParse(GUILayout.TextField(multiplier.ToString()), out multiplier);
                GUILayout.Label("Unassigned Kerbal Wages (at experience level 1)");
                int.TryParse(GUILayout.TextField(availableWages.ToString()), out availableWages);
                GUILayout.Label("Assigned Kerbal Wages (at experience level 1");
                int.TryParse(GUILayout.TextField(assignedWages.ToString()), out assignedWages);
                GUILayout.Label("Monthly cost per active vessel");
                int.TryParse(GUILayout.TextField(vesselCost.ToString()), out vesselCost);
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
        }
    }
}
