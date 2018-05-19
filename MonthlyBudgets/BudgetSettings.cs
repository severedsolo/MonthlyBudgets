using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

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
        public bool buildingCostsEnabled = true;
        public int buildingCosts = 476;
        public bool launchCostsEnabled = true;
        public int launchCostsVAB = 1000;
        public int launchCostsSPH = 100;
        PopupDialog settingsDialog;
        bool page1 = true;
        Rect geometry = new Rect(0.5f, 0.5f, 300, 300);
        string savedFile;
        void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
            savedFile = KSPUtil.ApplicationRootPath + "/GameData/MonthlyBudgets/PluginData/MonthlyBudgetDefaults.cfg";
        }

        private PopupDialog GenerateDialog()
        {
            List<DialogGUIBase> dialog = new List<DialogGUIBase>();
            DialogGUIBase[] horizontal = new DialogGUIBase[2];
            if (page1)
            {
                dialog.Add(new DialogGUILabel("MAIN SETTINGS"));
                dialog.Add(new DialogGUIToggle(() => masterSwitch, "Mod Enabled", (bool b) => { ToggleMasterSwitch(); }));
                if (masterSwitch)
                {
                    dialog.Add(new DialogGUIToggle(() => hardMode, "Penalty for not spending entire budget?", (bool b) => { hardMode = b; }));
                    dialog.Add(new DialogGUIToggle(() => contractInterceptor, "Contracts pay rep instead of funds?", (bool b) => { contractInterceptor = b; }));
                    dialog.Add(new DialogGUIToggle(() => coverCosts, "Always try to deduct costs from budget, even if current funds are higher?", (bool b) => { coverCosts = b; }));
                    dialog.Add(new DialogGUIToggle(() => stopTimewarp, "Stop Timewarp / Set KAC Alarm on budget", (bool b) => { stopTimewarp = b; }));
                    dialog.Add(new DialogGUIToggle(() => decayEnabled, "Decay Reputation each budget?", (bool b) => { decayEnabled = b; }));
                    dialog.Add(new DialogGUIToggle(() => buildingCostsEnabled, "Enable maintenance costs for buildings", (bool b) => { buildingCostsEnabled = b; }));
                    dialog.Add(new DialogGUIToggle(() => launchCostsEnabled, "Enable maintenance costs for launches", (bool b) => { launchCostsEnabled = b; }));
                }
            }
            else
            {
                dialog.Add(new DialogGUILabel("VALUES"));
                if (masterSwitch)
                {
                    if (decayEnabled)
                    {
                        horizontal[0] = new DialogGUILabel(delegate { return "Decay per budget: " + repDecay + "%"; }, false, false);
                        horizontal[1] = new DialogGUISlider(delegate { return repDecay; }, 0.0f, 100.0f, true, 280.0f, 30.0f, (float newValue) => { repDecay = (int)newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    }
                    horizontal[0] = new DialogGUILabel(delegate { return "Budget Interval: " + friendlyInterval + " days"; }, false, false);
                    horizontal[1] = new DialogGUISlider(delegate { return friendlyInterval; }, 0.0f, 427.0f, true, 280.0f, 30.0f, (float newValue) => { friendlyInterval = newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    horizontal[0] = new DialogGUILabel(delegate { return "Budget Multiplier: " + multiplier; }, false, false);
                    horizontal[1] = new DialogGUISlider(delegate { return multiplier; }, 0.0f, 9999.0f, true, 280.0f, 30.0f, (float newValue) => { multiplier = (int)newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    horizontal[0] = new DialogGUILabel(delegate { return "Unassigned Kerbal Base Wage: $" + availableWages; }, false, false);
                    horizontal[1] = new DialogGUISlider(delegate { return availableWages; }, 0.0f, 100000.0f, true, 280.0f, 30.0f, (float newValue) => { availableWages = (int)newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    horizontal[0] = new DialogGUILabel(delegate { return "Assigned Kerbal Base Wage: $" + assignedWages; }, false, false);
                    horizontal[1] = new DialogGUISlider(delegate { return assignedWages; }, 0.0f, 100000.0f, true, 280.0f, 30.0f, (float newValue) => { assignedWages = (int)newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    horizontal[0] = new DialogGUILabel(delegate { return "Monthly Cost per active vessel: $" + vesselCost; }, false, false);
                    horizontal[1] = new DialogGUISlider(delegate { return vesselCost; }, 0.0f, 100000.0f, true, 280.0f, 30.0f, (float newValue) => { vesselCost = (int)newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    if (buildingCostsEnabled)
                    {
                        horizontal[0] = new DialogGUILabel(delegate { return "Base Cost Per Building: $" + buildingCosts; }, false, false);
                        horizontal[1] = new DialogGUISlider(delegate { return buildingCosts; }, 0.0f, 10000.0f, true, 280.0f, 30.0f, (float newValue) => { buildingCosts = (int)newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    }
                    if (launchCostsEnabled)
                    {
                        horizontal[0] = new DialogGUILabel(delegate { return "Base Cost Per Launch(SPH): $" + launchCostsSPH; }, false, false);
                        horizontal[1] = new DialogGUISlider(delegate { return launchCostsSPH; }, 0.0f, 10000.0f, true, 280.0f, 30.0f, (float newValue) => { launchCostsSPH = (int)newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(delegate { return "Base Cost Per Launch(VAB): $" + launchCostsVAB; }, false, false);
                        horizontal[1] = new DialogGUISlider(delegate { return launchCostsVAB; }, 0.0f, 10000.0f, true, 280.0f, 30.0f, (float newValue) => { launchCostsVAB = (int)newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    }
                }
            }
            horizontal[0] = new DialogGUIButton("Switch Page", () => SwitchPage(), false);
            horizontal[1] = new DialogGUIButton("Close", () => CloseDialog(), false);
            dialog.Add(new DialogGUIHorizontalLayout(horizontal));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("MonthlyBudgetsDialog", "", "Monthly Budgets", UISkinManager.defaultSkin, geometry, dialog.ToArray()), true, UISkinManager.defaultSkin);
        }

        private void ToggleMasterSwitch()
        {
            masterSwitch = !masterSwitch;
            CloseDialog();
            Invoke("SpawnSettingsDialog", 0.1f);
        }

        private void SwitchPage()
        {
            page1 = !page1;
            CloseDialog();
            Invoke("SpawnSettingsDialog", 0.1f);
        }

        public void SpawnSettingsDialog()
        {
                if (settingsDialog == null) settingsDialog = GenerateDialog();
        }

        void CloseDialog()
        {
            Vector3 rt = settingsDialog.RTrf.position;
            geometry = new Rect(rt.x / Screen.width + 0.5f, rt.y / Screen.height + 0.5f, 600, 300);
            settingsDialog.Dismiss();
        }

        public void FirstRun()
        {
            firstRun = false;;
            if (!File.Exists(savedFile)) return;
            ConfigNode settings = ConfigNode.Load(savedFile);
            bool.TryParse(settings.GetValue("masterSwitch"), out masterSwitch);
            bool.TryParse(settings.GetValue("hardMode"), out hardMode);
            bool.TryParse(settings.GetValue("contractInterceptor"), out contractInterceptor);
            bool.TryParse(settings.GetValue("coverCosts"), out coverCosts);
            bool.TryParse(settings.GetValue("stopTimewarp"), out stopTimewarp);
            bool.TryParse(settings.GetValue("decayEnabled"), out decayEnabled);
            float.TryParse(settings.GetValue("friendlyInterval"), out friendlyInterval);
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
            SpawnSettingsDialog();
        }
    }
}
