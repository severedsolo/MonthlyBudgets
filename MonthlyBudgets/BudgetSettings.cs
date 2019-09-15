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
        public bool upgraded = false;
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
        public int vesselCost = 1000;
        public bool firstRun = true;
        public bool buildingCostsEnabled = true;
        public bool launchCostsEnabled = true;
        public int launchCostsVAB = 1000;
        public int launchCostsSPH = 100;
        public int sphCost = 8000;
        public int missionControlCost = 6000;
        public int astronautComplexCost = 2000;
        public int administrationCost = 4000;
        public int vabCost = 8000;
        public int trackingStationCost = 4000;
        public int rndCost = 8000;
        public int otherFacilityCost = 5000;
        public int vesselDeathPenalty = 15;
        public int kerbalDeathPenalty = 10;
        private PopupDialog settingsDialog;
        private bool page1 = true;
        private Rect geometry = new Rect(0.5f, 0.5f, 300, 300);
        private string savedFile;
        private  void Awake()
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
                        horizontal[0] = new DialogGUILabel(delegate { return "Base Cost for SPH: $" + sphCost; }, false, false);
                        horizontal[1] = new DialogGUISlider(delegate { return sphCost; }, 0.0f, 10000.0f, true, 280.0f, 30.0f, (float newValue) => { sphCost = (int)newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(delegate { return "Base Cost for Mission Control: $" + missionControlCost; }, false, false);
                        horizontal[1] = new DialogGUISlider(delegate { return missionControlCost; }, 0.0f, 10000.0f, true, 280.0f, 30.0f, (float newValue) => { missionControlCost = (int)newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(delegate { return "Base Cost for Astronaut Complex: $" + astronautComplexCost; }, false, false);
                        horizontal[1] = new DialogGUISlider(delegate { return astronautComplexCost; }, 0.0f, 10000.0f, true, 280.0f, 30.0f, (float newValue) => { astronautComplexCost = (int)newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(delegate { return "Base Cost for Administration: $" + administrationCost; }, false, false);
                        horizontal[1] = new DialogGUISlider(delegate { return administrationCost; }, 0.0f, 10000.0f, true, 280.0f, 30.0f, (float newValue) => { administrationCost = (int)newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(delegate { return "Base Cost for VAB: $" + vabCost; }, false, false);
                        horizontal[1] = new DialogGUISlider(delegate { return vabCost; }, 0.0f, 10000.0f, true, 280.0f, 30.0f, (float newValue) => { vabCost = (int)newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(delegate { return "Base Cost for Tracking Station: $" + trackingStationCost; }, false, false);
                        horizontal[1] = new DialogGUISlider(delegate { return trackingStationCost; }, 0.0f, 10000.0f, true, 280.0f, 30.0f, (float newValue) => { trackingStationCost = (int)newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(delegate { return "Base Cost for R&D: $" + rndCost; }, false, false);
                        horizontal[1] = new DialogGUISlider(delegate { return rndCost; }, 0.0f, 10000.0f, true, 280.0f, 30.0f, (float newValue) => { rndCost = (int)newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(delegate { return "Base Cost for Other Facilities (non-stock): $" + otherFacilityCost; }, false, false);
                        horizontal[1] = new DialogGUISlider(delegate { return otherFacilityCost; }, 0.0f, 10000.0f, true, 280.0f, 30.0f, (float newValue) => { otherFacilityCost = (int)newValue; });
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
                    horizontal[0] = new DialogGUILabel(delegate { return "Crewed Vessel Loss Penalty " + vesselDeathPenalty; }, false, false);
                    horizontal[1] = new DialogGUISlider(delegate { return vesselDeathPenalty; }, 0.0f, 100f, true, 280.0f, 30.0f, newValue => { vesselDeathPenalty = (int)newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    horizontal[0] = new DialogGUILabel(delegate { return "Per Kerbal Death Penalty " + kerbalDeathPenalty; }, false, false);
                    horizontal[1] = new DialogGUISlider(delegate { return kerbalDeathPenalty; }, 0.0f, 100f, true, 280.0f, 30.0f, newValue => { kerbalDeathPenalty = (int)newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
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

        public int ReturnBuildingCosts(SpaceCenterFacility facility)
        {
            switch(facility)
            {
                case SpaceCenterFacility.Administration:
                    return administrationCost;
                case SpaceCenterFacility.AstronautComplex:
                    return astronautComplexCost;
                case SpaceCenterFacility.MissionControl:
                    return missionControlCost;
                case SpaceCenterFacility.ResearchAndDevelopment:
                    return rndCost;
                case SpaceCenterFacility.SpaceplaneHangar:
                    return sphCost;
                case SpaceCenterFacility.TrackingStation:
                    return trackingStationCost;
                case SpaceCenterFacility.VehicleAssemblyBuilding:
                    return vabCost;
                default:
                    return otherFacilityCost;
            }
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
            bool.TryParse(settings.GetValue("launchCostsEnabled"), out launchCostsEnabled);
            int.TryParse(settings.GetValue("launchCostsVAB"), out launchCostsVAB);
            int.TryParse(settings.GetValue("launchCostsSPH"), out launchCostsSPH);
            int.TryParse(settings.GetValue("sphCost"), out sphCost);
            int.TryParse(settings.GetValue("missionControlCost"), out missionControlCost);
            int.TryParse(settings.GetValue("astronautComplexCost"), out astronautComplexCost);
            int.TryParse(settings.GetValue("administrationCost"), out administrationCost);
            int.TryParse(settings.GetValue("vabCost"), out vabCost);
            int.TryParse(settings.GetValue("trackingStationCost"), out trackingStationCost);
            int.TryParse(settings.GetValue("rndCost"), out rndCost);
            int.TryParse(settings.GetValue("otherFacilityCost"), out otherFacilityCost);
            int.TryParse(settings.GetValue("kerbalDeathPenalty"), out kerbalDeathPenalty);
            int.TryParse(settings.GetValue("vesselDeathPenalty"), out vesselDeathPenalty);
            MonthlyBudgets.instance.BudgetAwarded(0, 0);
            upgraded = true;
            SpawnSettingsDialog();
        }
    }
}
