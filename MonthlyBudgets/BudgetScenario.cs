using System.Collections.Generic;
using UnityEngine;

namespace MonthlyBudgets
{
    // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
    [KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames,
        GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    internal class BudgetScenario : ScenarioModule
    {
        private const string CurrentVersion = "4.11";
        private const string PreviousVersion = "4.8";
        private string _saveGameVersion = "0.0";
        private PopupDialog _errorDialog;
        private readonly Rect _geometry = new Rect(0.5f, 0.5f, 800, 30);

        public override void OnSave(ConfigNode savedNode)
        {
            savedNode.SetValue("saveGameVersion", CurrentVersion, true);
            savedNode.SetValue("LastBudgetUpdate", MonthlyBudgets.instance.lastUpdate, true);
            savedNode.SetValue("EmergencyFund", MonthlyBudgets.instance.emergencyBudget, true);
            savedNode.SetValue("EmergencyFundPercent", MonthlyBudgets.instance.emergencyBudgetPercentage, true);
            savedNode.SetValue("EmergencyFundingEnabled", MonthlyBudgets.instance.enableEmergencyBudget, true);
            savedNode.SetValue("MasterSwitch", BudgetSettings.instance.masterSwitch, true);
            savedNode.SetValue("HardMode", BudgetSettings.instance.hardMode, true);
            savedNode.SetValue("ContractInterceptor", BudgetSettings.instance.contractInterceptor, true);
            savedNode.SetValue("CoverCosts", BudgetSettings.instance.coverCosts, true);
            savedNode.SetValue("StopTimeWarp", BudgetSettings.instance.stopTimewarp, true);
            savedNode.SetValue("DecayEnabled", BudgetSettings.instance.decayEnabled, true);
            savedNode.SetValue("RepDecay", BudgetSettings.instance.repDecay, true);
            savedNode.SetValue("Multiplier", BudgetSettings.instance.multiplier, true);
            savedNode.SetValue("FriendlyInterval", BudgetSettings.instance.friendlyInterval, true);
            savedNode.SetValue("AvailableWages", BudgetSettings.instance.availableWages, true);
            savedNode.SetValue("AssignedWages", BudgetSettings.instance.assignedWages, true);
            savedNode.SetValue("VesselCost", BudgetSettings.instance.vesselCost, true);
            savedNode.SetValue("FirstRun", BudgetSettings.instance.firstRun, true);
            savedNode.SetValue("RnD", MonthlyBudgets.instance.researchBudget, true);
            savedNode.SetValue("JokeSeen", MonthlyBudgets.instance.jokeSeen, true);
            savedNode.SetValue("BuildingCostsEnabled", BudgetSettings.instance.buildingCostsEnabled, true);
            savedNode.SetValue("sphCost", BudgetSettings.instance.sphCost, true);
            savedNode.SetValue("missionControlCost", BudgetSettings.instance.missionControlCost, true);
            savedNode.SetValue("astronautComplexCost", BudgetSettings.instance.astronautComplexCost, true);
            savedNode.SetValue("administrationCost", BudgetSettings.instance.administrationCost, true);
            savedNode.SetValue("vabCost", BudgetSettings.instance.vabCost, true);
            savedNode.SetValue("trackingStationCost", BudgetSettings.instance.trackingStationCost, true);
            savedNode.SetValue("rndCost", BudgetSettings.instance.rndCost, true);
            savedNode.SetValue("otherFacilityCost", BudgetSettings.instance.otherFacilityCost, true);
            savedNode.SetValue("LaunchCostsEnabled", BudgetSettings.instance.launchCostsEnabled, true);
            savedNode.SetValue("LaunchCostsVAB", BudgetSettings.instance.launchCostsVab, true);
            savedNode.SetValue("LaunchCostsSPH", BudgetSettings.instance.launchCostsSph, true);
            savedNode.SetValue("LaunchCosts", MonthlyBudgets.instance.launchCosts, true);
            savedNode.SetValue("kerbalDeathPenalty", BudgetSettings.instance.kerbalDeathPenalty, true);
            savedNode.SetValue("vesselDeathPenalty", BudgetSettings.instance.vesselDeathPenalty, true);
            savedNode.SetValue("upgraded", BudgetSettings.instance.upgraded, true);
            savedNode.SetValue("useItOrLoseIt", BudgetSettings.instance.useItOrLoseIt, true);
        }

        public override void OnLoad(ConfigNode node)
        {
            node.TryGetValue("saveGameVersion", ref _saveGameVersion);
            if (_saveGameVersion != CurrentVersion && _saveGameVersion != PreviousVersion && !BudgetSettings.instance.firstRun)
            {
                SpawnErrorDialog();
                return;
            }
            node.TryGetValue("LastBudgetUpdate", ref MonthlyBudgets.instance.lastUpdate);
            node.TryGetValue("EmergencyFund", ref MonthlyBudgets.instance.emergencyBudget);
            node.TryGetValue("EmergencyFundPercent", ref MonthlyBudgets.instance.emergencyBudgetPercentage);
            node.TryGetValue("EmergencyFundingEnabled", ref MonthlyBudgets.instance.enableEmergencyBudget);
            node.TryGetValue("MasterSwitch", ref BudgetSettings.instance.masterSwitch);
            node.TryGetValue("HardMode", ref BudgetSettings.instance.hardMode);
            node.TryGetValue("ContractInterceptor", ref BudgetSettings.instance.contractInterceptor);
            node.TryGetValue("CoverCosts", ref BudgetSettings.instance.coverCosts);
            node.TryGetValue("StopTimeWarp", ref BudgetSettings.instance.stopTimewarp);
            node.TryGetValue("DecayEnabled", ref BudgetSettings.instance.decayEnabled);
            node.TryGetValue("RepDecay", ref BudgetSettings.instance.repDecay);
            node.TryGetValue("Multiplier", ref BudgetSettings.instance.multiplier);
            node.TryGetValue("FriendlyInterval", ref BudgetSettings.instance.friendlyInterval);
            node.TryGetValue("AvailableWages", ref BudgetSettings.instance.availableWages);
            node.TryGetValue("AssignedWages", ref BudgetSettings.instance.assignedWages);
            node.TryGetValue("VesselCost", ref BudgetSettings.instance.vesselCost);
            node.TryGetValue("FirstRun", ref BudgetSettings.instance.firstRun);
            node.TryGetValue("RnD", ref MonthlyBudgets.instance.researchBudget);
            node.TryGetValue("JokeSeen", ref MonthlyBudgets.instance.jokeSeen);
            node.TryGetValue("BuildingCostsEnabled", ref BudgetSettings.instance.buildingCostsEnabled);
            node.TryGetValue("sphCost", ref BudgetSettings.instance.sphCost);
            node.TryGetValue("missionControlCost", ref BudgetSettings.instance.missionControlCost);
            node.TryGetValue("astronautComplexCost", ref BudgetSettings.instance.astronautComplexCost);
            node.TryGetValue("administrationCost", ref BudgetSettings.instance.administrationCost);
            node.TryGetValue("vabCost", ref BudgetSettings.instance.vabCost);
            node.TryGetValue("trackingStationCost", ref BudgetSettings.instance.trackingStationCost);
            node.TryGetValue("rndCost", ref BudgetSettings.instance.rndCost);
            node.TryGetValue("otherFacilityCost", ref BudgetSettings.instance.otherFacilityCost);
            node.TryGetValue("LaunchCostsEnabled", ref BudgetSettings.instance.launchCostsEnabled);
            node.TryGetValue("LaunchCostsVAB", ref BudgetSettings.instance.launchCostsVab);
            node.TryGetValue("LaunchCostsSPH", ref BudgetSettings.instance.launchCostsSph);
            node.TryGetValue("upgraded", ref BudgetSettings.instance.upgraded);
            node.TryGetValue("LaunchCosts", ref MonthlyBudgets.instance.launchCosts);
            node.TryGetValue("kerbalDeathPenalty", ref BudgetSettings.instance.kerbalDeathPenalty);
            node.TryGetValue("vesselDeathPenalty", ref BudgetSettings.instance.vesselDeathPenalty);
            if (_saveGameVersion == "4.11")
            {
                node.TryGetValue("useItOrLoseIt", ref BudgetSettings.instance.useItOrLoseIt);
            }
            if (BudgetSettings.instance.firstRun || !BudgetSettings.instance.upgraded)
                BudgetSettings.instance.FirstRun();
        }

        private void SpawnErrorDialog()
        {
            if (_errorDialog == null) _errorDialog = GenerateDialog();
        }

        private PopupDialog GenerateDialog()
        {
            List<DialogGUIBase> dialog = new List<DialogGUIBase>
            {
                new DialogGUILabel(
                    "Couldn't detect an upgrade path for your save. Your settings have been reset to default"),
                new DialogGUIButton("OK", ResetSettings, false)
            };
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("ErrorDialog", "", "Monthly Budgets", UISkinManager.defaultSkin, _geometry,
                    dialog.ToArray()), false, UISkinManager.defaultSkin);
        }

        private void ResetSettings()
        {
            _errorDialog.Dismiss();
            BudgetSettings.instance.FirstRun();
        }
    }
}