using UnityEngine;

namespace MonthlyBudgets
{
    class BudgetSettings : GameParameters.CustomParameterNode
    {
        public enum BudgetDifficulty
        {
            EASY,
            MEDIUM,
            HARD,
        }
        public override string Title { get { return "Monthly Budget Options"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.CAREER; } }
        public override string Section { get { return "Monthly Budgets"; } }
        public override string DisplaySection { get { return Section; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }
        public bool autoPersistance = true;
        public bool newGameOnly = false;
        [GameParameters.CustomParameterUI("Mod enabled?")]
        public bool masterSwitch = true;
        [GameParameters.CustomParameterUI("Enable Hard Mode?", toolTip = "Removes some reputation if you have leftover funds at the end of a budget cycle")]
        public bool HardMode = false;
        [GameParameters.CustomParameterUI("Enable Reputation Decay?", toolTip = "Repuation naturally decreases over time")]
        public bool DecayEnabled = false;
        [GameParameters.CustomParameterUI("Disable Contract Funding?", toolTip = "Converts contract funding rewards to reputation")]
        public bool ContractInterceptor = true;
        [GameParameters.CustomParameterUI("Disable Progression Funding?", toolTip = "Converts records (distance/speed) funding rewards to reputation")]
        public bool DisableRecords = true;
        [GameParameters.CustomParameterUI("Taxpayers always try to cover costs", toolTip = "When enabled, costs will always try to be deducted from the budget, even if funds are higher than the awarded budget")]
        public bool coverCosts = false;
        [GameParameters.CustomParameterUI("Stop Timewarp on budget?", toolTip = "Will also add KAC alarm if applicable")]
        public bool stopTimewarp = false;
        [GameParameters.CustomIntParameterUI("Decay percentage", minValue = 1, maxValue = 100, toolTip = "How much to decay the repuation by each month (if Reputation Decay is switched on)")]
        public int RepDecay = 10;
        [GameParameters.CustomIntParameterUI("Multiplier", minValue = 1, maxValue = 9999)]
        public int  Multiplier = 2227;
        [GameParameters.CustomFloatParameterUI("Budget Interval", minValue = 1, maxValue = 427)]
        public float friendlyInterval = 30;
        [GameParameters.CustomIntParameterUI("Unassigned Kerbal Wages", minValue = 1000, maxValue = 100000)]
        public int availableWages = 1000;
        [GameParameters.CustomIntParameterUI("Assigned Kerbal Wages", minValue = 1000, maxValue = 100000)]
        public int assignedWages = 2000;
        [GameParameters.CustomIntParameterUI("Vessel Maintenance cost", minValue = 1000, maxValue = 100000)]
        public int vesselCost = 10000;
        


        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            Debug.Log("[MonthlyBudgets]: Setting difficulty preset");
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    Multiplier = 5000;
                    availableWages = 500;
                    assignedWages = 1000;
                    vesselCost = 5000;
                    break;

                case GameParameters.Preset.Normal:
                    Multiplier = 2227;
                    availableWages = 1000;
                    assignedWages = 2000;
                    vesselCost = 10000;
                    break;

                case GameParameters.Preset.Moderate:
                    Multiplier = 1000;
                    availableWages = 3500;
                    assignedWages = 6000;
                    vesselCost = 12000;
                    DecayEnabled = true;
                    break;

                case GameParameters.Preset.Hard:
                    Multiplier = 500;
                    availableWages = 5000;
                    assignedWages = 10000;
                    vesselCost = 20000;
                    DecayEnabled = true;
                    HardMode = true;
                    break;
            }
        }
    }
}
