using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MonthlyBudgets
{
    [KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames,
GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    class BudgetScenario : ScenarioModule
    {
        public override void OnSave(ConfigNode savedNode)
        {
            savedNode.SetValue("LastBudgetUpdate", MonthlyBudgets.instance.lastUpdate, true);
            savedNode.SetValue("EmergencyFund", MonthlyBudgets.instance.emergencyBudget, true);
            savedNode.SetValue("EmergencyFundPercent", MonthlyBudgets.instance.emergencyBudgetPercentage, true);
            savedNode.SetValue("EmergencyFundingEnabled", MonthlyBudgets.instance.enableEmergencyBudget, true);
            savedNode.SetValue("MasterSwitch", BudgetSettings.instance.masterSwitch, true);
            savedNode.SetValue("HardMode", BudgetSettings.instance.hardMode, true);
            savedNode.SetValue("ContractInterceptor", BudgetSettings.instance.contractInterceptor, true);
            savedNode.SetValue("CoverCosts", BudgetSettings.instance.coverCosts, true);
            savedNode.SetValue("StopTimeWarp", BudgetSettings.instance.stopTimewarp, true);
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
            savedNode.SetValue("BuildingCosts", BudgetSettings.instance.buildingCosts, true);
            savedNode.SetValue("LaunchCostsEnabled", BudgetSettings.instance.launchCostsEnabled, true);
            savedNode.SetValue("LaunchCostsVAB", BudgetSettings.instance.launchCostsVAB, true);
            savedNode.SetValue("LaunchCostsSPH", BudgetSettings.instance.launchCostsSPH, true);
            savedNode.SetValue("LaunchCosts", MonthlyBudgets.instance.launchCosts, true);
        }

        public override void OnLoad(ConfigNode node)
        {
            node.TryGetValue("LastBudgetUpdate", ref MonthlyBudgets.instance.lastUpdate);
            node.TryGetValue("EmergencyFund", ref MonthlyBudgets.instance.emergencyBudget);
            node.TryGetValue("EmergencyFundPercent", ref MonthlyBudgets.instance.emergencyBudgetPercentage);
            node.TryGetValue("EmergencyFundingEnabled", ref MonthlyBudgets.instance.enableEmergencyBudget);
            node.TryGetValue("MasterSwitch", ref BudgetSettings.instance.masterSwitch);
            node.TryGetValue("HardMode", ref BudgetSettings.instance.hardMode);
            node.TryGetValue("ContractInterceptor", ref BudgetSettings.instance.contractInterceptor);
            node.TryGetValue("CoverCosts", ref BudgetSettings.instance.coverCosts);
            node.TryGetValue("StopTimeWarp", ref BudgetSettings.instance.stopTimewarp);
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
            node.TryGetValue("BuildingCosts", ref BudgetSettings.instance.buildingCosts);
            node.TryGetValue("LaunchCostsEnabled", ref BudgetSettings.instance.launchCostsEnabled);
            node.TryGetValue("LaunchCostsVAB", ref BudgetSettings.instance.launchCostsVAB);
            node.TryGetValue("LaunchCostsSPH", ref BudgetSettings.instance.launchCostsSPH);
            node.TryGetValue("LaunchCosts", ref MonthlyBudgets.instance.launchCosts);
            MonthlyBudgets.instance.inputString = MonthlyBudgets.instance.emergencyBudgetPercentage.ToString();
            if (BudgetSettings.instance.firstRun) BudgetSettings.instance.FirstRun();
        }
    }
}
