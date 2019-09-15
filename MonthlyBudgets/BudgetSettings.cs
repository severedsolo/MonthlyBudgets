using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonthlyBudgets
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    internal class BudgetSettings : MonoBehaviour
    {
        public static BudgetSettings instance;
        public int administrationCost = 4000;
        public int assignedWages = 2000;
        public int astronautComplexCost = 2000;
        public int availableWages = 1000;
        public bool buildingCostsEnabled = true;
        public bool contractInterceptor = true;
        public bool coverCosts;
        public bool decayEnabled;
        public bool firstRun = true;
        // ReSharper disable once RedundantDefaultMemberInitializer
        public bool useItOrLoseIt = false;
        public float friendlyInterval = 30;
        private Rect _geometry = new Rect(0.5f, 0.5f, 300, 300);
        public bool hardMode;
        public int kerbalDeathPenalty = 10;
        public bool launchCostsEnabled = true;
        [FormerlySerializedAs("launchCostsSPH")] public int launchCostsSph = 100;
        [FormerlySerializedAs("launchCostsVAB")] public int launchCostsVab = 1000;
        public bool masterSwitch = true;
        public int missionControlCost = 6000;
        public int multiplier = 2227;
        public int otherFacilityCost = 5000;
        private bool _page1 = true;
        public int repDecay = 10;
        public int rndCost = 8000;
        private string _savedFile;
        private PopupDialog _settingsDialog;
        public int sphCost = 8000;
        public bool stopTimewarp = true;
        public int trackingStationCost = 4000;
        public bool upgraded;
        public int vabCost = 8000;
        public int vesselCost = 1000;
        public int vesselDeathPenalty = 15;

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
            _savedFile = KSPUtil.ApplicationRootPath + "/GameData/MonthlyBudgets/PluginData/MonthlyBudgetDefaults.cfg";
        }

        private PopupDialog GenerateDialog()
        {
            List<DialogGUIBase> dialog = new List<DialogGUIBase>();
            DialogGUIBase[] horizontal = new DialogGUIBase[2];
            if (_page1)
            {
                dialog.Add(new DialogGUILabel("MAIN SETTINGS"));
                dialog.Add(new DialogGUIToggle(() => masterSwitch, "Mod Enabled", b => { ToggleMasterSwitch(); }));
                if (masterSwitch)
                {
                    dialog.Add(new DialogGUIToggle(() => useItOrLoseIt, "Use it Or Lose It",
                        b => { useItOrLoseIt = b; }));
                    dialog.Add(new DialogGUIToggle(() => hardMode, "Penalty for not spending entire budget?",
                        b => { hardMode = b; }));
                    dialog.Add(new DialogGUIToggle(() => contractInterceptor, "Contracts pay rep instead of funds?",
                        b => { contractInterceptor = b; }));
                    dialog.Add(new DialogGUIToggle(() => coverCosts,
                        "Always try to deduct costs from budget, even if current funds are higher?",
                        b => { coverCosts = b; }));
                    dialog.Add(new DialogGUIToggle(() => stopTimewarp, "Stop Timewarp / Set KAC Alarm on budget",
                        b => { stopTimewarp = b; }));
                    dialog.Add(new DialogGUIToggle(() => decayEnabled, "Decay Reputation each budget?",
                        b => { decayEnabled = b; }));
                    dialog.Add(new DialogGUIToggle(() => buildingCostsEnabled, "Enable maintenance costs for buildings",
                        b => { buildingCostsEnabled = b; }));
                    dialog.Add(new DialogGUIToggle(() => launchCostsEnabled, "Enable maintenance costs for launches",
                        b => { launchCostsEnabled = b; }));
                }
            }
            else
            {
                dialog.Add(new DialogGUILabel("VALUES"));
                if (masterSwitch)
                {
                    if (decayEnabled)
                    {
                        horizontal[0] = new DialogGUILabel(() => "Decay per budget: " + repDecay + "%");
                        horizontal[1] = new DialogGUISlider(() => repDecay, 0.0f, 100.0f, true, 280.0f, 30.0f,
                            newValue => { repDecay = (int) newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    }

                    horizontal[0] = new DialogGUILabel(() => "Budget Interval: " + friendlyInterval + " days");
                    horizontal[1] = new DialogGUISlider(() => friendlyInterval, 0.0f, 427.0f, true,
                        280.0f, 30.0f, newValue => { friendlyInterval = newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    horizontal[0] = new DialogGUILabel(() => "Budget Multiplier: " + multiplier);
                    horizontal[1] = new DialogGUISlider(() => multiplier, 0.0f, 9999.0f, true, 280.0f,
                        30.0f, newValue => { multiplier = (int) newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    horizontal[0] = new DialogGUILabel(() => "Unassigned Kerbal Base Wage: $" + availableWages);
                    horizontal[1] = new DialogGUISlider(() => availableWages, 0.0f, 100000.0f, true,
                        280.0f, 30.0f, newValue => { availableWages = (int) newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    horizontal[0] = new DialogGUILabel(() => "Assigned Kerbal Base Wage: $" + assignedWages);
                    horizontal[1] = new DialogGUISlider(() => assignedWages, 0.0f, 100000.0f, true,
                        280.0f, 30.0f, newValue => { assignedWages = (int) newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    horizontal[0] = new DialogGUILabel(() => "Monthly Cost per active vessel: $" + vesselCost);
                    horizontal[1] = new DialogGUISlider(() => vesselCost, 0.0f, 100000.0f, true, 280.0f,
                        30.0f, newValue => { vesselCost = (int) newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    if (buildingCostsEnabled)
                    {
                        horizontal[0] = new DialogGUILabel(() => "Base Cost for SPH: $" + sphCost);
                        horizontal[1] = new DialogGUISlider(() => sphCost, 0.0f, 10000.0f, true, 280.0f,
                            30.0f, newValue => { sphCost = (int) newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(
                            () => "Base Cost for Mission Control: $" + missionControlCost);
                        horizontal[1] = new DialogGUISlider(() => missionControlCost, 0.0f, 10000.0f,
                            true, 280.0f, 30.0f, newValue => { missionControlCost = (int) newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(() =>
                            "Base Cost for Astronaut Complex: $" + astronautComplexCost);
                        horizontal[1] = new DialogGUISlider(() => astronautComplexCost, 0.0f, 10000.0f,
                            true, 280.0f, 30.0f, newValue => { astronautComplexCost = (int) newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(() => "Base Cost for Administration: $" + administrationCost);
                        horizontal[1] = new DialogGUISlider(() => administrationCost, 0.0f, 10000.0f,
                            true, 280.0f, 30.0f, newValue => { administrationCost = (int) newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(() => "Base Cost for VAB: $" + vabCost);
                        horizontal[1] = new DialogGUISlider(() => vabCost, 0.0f, 10000.0f, true, 280.0f,
                            30.0f, newValue => { vabCost = (int) newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(() =>
                            "Base Cost for Tracking Station: $" + trackingStationCost);
                        horizontal[1] = new DialogGUISlider(() => trackingStationCost, 0.0f, 10000.0f,
                            true, 280.0f, 30.0f, newValue => { trackingStationCost = (int) newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(() => "Base Cost for R&D: $" + rndCost);
                        horizontal[1] = new DialogGUISlider(() => rndCost, 0.0f, 10000.0f, true, 280.0f,
                            30.0f, newValue => { rndCost = (int) newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(() =>
                            "Base Cost for Other Facilities (non-stock): $" + otherFacilityCost);
                        horizontal[1] = new DialogGUISlider(() => otherFacilityCost, 0.0f, 10000.0f,
                            true, 280.0f, 30.0f, newValue => { otherFacilityCost = (int) newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    }

                    if (launchCostsEnabled)
                    {
                        horizontal[0] = new DialogGUILabel(() => "Base Cost Per Launch(SPH): $" + launchCostsSph);
                        horizontal[1] = new DialogGUISlider(() => launchCostsSph, 0.0f, 10000.0f, true,
                            280.0f, 30.0f, newValue => { launchCostsSph = (int) newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                        horizontal[0] = new DialogGUILabel(() => "Base Cost Per Launch(VAB): $" + launchCostsVab);
                        horizontal[1] = new DialogGUISlider(() => launchCostsVab, 0.0f, 10000.0f, true,
                            280.0f, 30.0f, newValue => { launchCostsVab = (int) newValue; });
                        dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    }

                    horizontal[0] = new DialogGUILabel(() => "Crewed Vessel Loss Penalty " + vesselDeathPenalty);
                    horizontal[1] = new DialogGUISlider(() => vesselDeathPenalty, 0.0f, 100f, true,
                        280.0f, 30.0f, newValue => { vesselDeathPenalty = (int) newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                    horizontal[0] = new DialogGUILabel(() => "Per Kerbal Death Penalty " + kerbalDeathPenalty);
                    horizontal[1] = new DialogGUISlider(() => kerbalDeathPenalty, 0.0f, 100f, true,
                        280.0f, 30.0f, newValue => { kerbalDeathPenalty = (int) newValue; });
                    dialog.Add(new DialogGUIHorizontalLayout(horizontal));
                }
            }

            horizontal[0] = new DialogGUIButton("Switch Page", SwitchPage, false);
            horizontal[1] = new DialogGUIButton("Close", CloseDialog, false);
            dialog.Add(new DialogGUIHorizontalLayout(horizontal));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("MonthlyBudgetsDialog", "", "Monthly Budgets", UISkinManager.defaultSkin,
                    _geometry, dialog.ToArray()), true, UISkinManager.defaultSkin);
        }

        private void ToggleMasterSwitch()
        {
            masterSwitch = !masterSwitch;
            CloseDialog();
            Invoke(nameof(SpawnSettingsDialog), 0.1f);
        }

        private void SwitchPage()
        {
            _page1 = !_page1;
            CloseDialog();
            Invoke(nameof(SpawnSettingsDialog), 0.1f);
        }

        public void SpawnSettingsDialog()
        {
            if (_settingsDialog == null) _settingsDialog = GenerateDialog();
        }

        private void CloseDialog()
        {
            Vector3 rt = _settingsDialog.RTrf.position;
            _geometry = new Rect(rt.x / Screen.width + 0.5f, rt.y / Screen.height + 0.5f, 600, 300);
            _settingsDialog.Dismiss();
        }

        public int ReturnBuildingCosts(SpaceCenterFacility facility)
        {
            switch (facility)
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
                case SpaceCenterFacility.LaunchPad:
                    return 0;
                case SpaceCenterFacility.Runway:
                    return 0;
                default:
                    return otherFacilityCost;
            }
        }

        public void FirstRun()
        {
            firstRun = false;
            if (!File.Exists(_savedFile)) return;
            ConfigNode settings = ConfigNode.Load(_savedFile);
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
            int.TryParse(settings.GetValue("launchCostsVAB"), out launchCostsVab);
            int.TryParse(settings.GetValue("launchCostsSPH"), out launchCostsSph);
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
            bool.TryParse(settings.GetValue("useItOrLoseIt"), out useItOrLoseIt);
            MonthlyBudgets.instance.BudgetAwarded(0, 0);
            upgraded = true;
            SpawnSettingsDialog();
        }
    }
}