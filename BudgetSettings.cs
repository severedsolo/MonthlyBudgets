using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace severedsolo
{
    class BudgetSettings : GameParameters.CustomParameterNode
    {
        public enum BudgetDifficulty
        {
            EASY,
            MEDIUM,
            HARD,
        }

        public override string Title { get { return "MonthlyBudgetOptions"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.CAREER; } }
        public override string Section { get { return "MonthlyBudgets"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return true; } }
        public bool autoPersistance = true;
        public bool newGameOnly = false;
        [GameParameters.CustomParameterUI("Enable Hard Mode?", toolTip = "removes some reputation if you have leftover funds at the end of a budget cycle")]
        public bool HardMode = false;
        [GameParameters.CustomIntParameterUI("Multiplier", minValue = 1, maxValue = 9999)]
        public int  Multiplier = 2227;
        [GameParameters.CustomFloatParameterUI("Budget Interval", minValue = 1, maxValue = 427)]
        public float friendlyInterval = 30;
        [GameParameters.CustomIntParameterUI("Unassigned Kerbal Wages", minValue = 1000, maxValue = 100000)]
        public int availableWages = 5000;
        [GameParameters.CustomIntParameterUI("Assigned Kerbal Wages", minValue = 1000, maxValue = 100000)]
        public int assignedWages = 10000;
        [GameParameters.CustomIntParameterUI("Vessel Maintenance cost", minValue = 1000, maxValue = 100000)]
        public int vesselCost = 10000;

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            Debug.Log("[MonthlyBudgets]: Setting difficulty preset");
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    Multiplier = 5000;
                    availableWages = 1000;
                    assignedWages = 5000;
                    vesselCost = 5000;
                    break;

                case GameParameters.Preset.Normal:
                    Multiplier = 2227;
                    availableWages = 5000;
                    assignedWages = 10000;
                    vesselCost = 10000;
                    break;

                case GameParameters.Preset.Moderate:
                    Multiplier = 1000;
                    availableWages = 7000;
                    assignedWages = 12000;
                    vesselCost = 12000;
                    break;

                case GameParameters.Preset.Hard:
                    Multiplier = 500;
                    availableWages = 10000;
                    assignedWages = 20000;
                    vesselCost = 20000;
                    break;
            }
        }
    }
}
