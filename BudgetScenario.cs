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
        }

        public override void OnLoad(ConfigNode node)
        {
            node.TryGetValue("LastBudgetUpdate", ref MonthlyBudgets.instance.lastUpdate);
            node.TryGetValue("EmergencyFund", ref MonthlyBudgets.instance.emergencyBudget);
            node.TryGetValue("EmergencyFundPercent", ref MonthlyBudgets.instance.emergencyBudgetPercentage);
            node.TryGetValue("EmergencyFundingEnabled", ref MonthlyBudgets.instance.enableEmergencyBudget);
            MonthlyBudgets.instance.inputString = MonthlyBudgets.instance.emergencyBudgetPercentage.ToString();
        }
    }
    [KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames,
GameScenes.FLIGHT)]
    class BudgetProgressionScenario : ScenarioModule
    {
        public override void OnSave(ConfigNode savedNode)
        {
            savedNode.SetValue("SpeedRecordIndex", RecordsReplacer.instance.speedRecordIndex, true);
            savedNode.SetValue("AltitudeRecordIndex", RecordsReplacer.instance.altitudeRecordIndex, true);
        }

        public override void OnLoad(ConfigNode node)
        {
            node.TryGetValue("SpeedRecordIndex", ref RecordsReplacer.instance.speedRecordIndex);
            node.TryGetValue("AltitudeRecordIndex", ref RecordsReplacer.instance.altitudeRecordIndex);;
        }
    }
}
