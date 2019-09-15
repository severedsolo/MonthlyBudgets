using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MonthlyBudgets_KACWrapper;
using UnityEngine;
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable Unity.PerformanceCriticalCodeInvocation

namespace MonthlyBudgets
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class MonthlyBudgets : MonoBehaviour
    {
        public static MonthlyBudgets instance;

        //OnBudgetAwarded fires when we award a budget. Overloads are budget, costs)
        // ReSharper disable once MemberCanBePrivate.Global
        public static EventData<double, double> onBudgetAwarded;
        public double dayLength;
        public double emergencyBudget;
        public float emergencyBudgetPercentage = 10;
        public bool enableEmergencyBudget;
        private SpaceCenterFacility[] _facilities;
        public CelestialBody homeWorld;
        public bool jokeSeen;
        public double lastUpdate;
        public int launchCosts;
        public float researchBudget;
        private bool _timeDiscrepancyLog = true;
        public double yearLength;

        private void Budget(double timeSinceLastUpdate)
        {
            try
            {
                double funds = Funding.Instance.Funds;
                float costs = 0;
                double offsetFunds = funds;
                if (BudgetSettings.instance.friendlyInterval * dayLength * 2 > timeSinceLastUpdate)
                {
                    if (BudgetSettings.instance.hardMode)
                    {
                        int penalty = (int) funds / 10000;
                        if (penalty < Reputation.CurrentRep)
                            Reputation.Instance.AddReputation(-penalty, TransactionReasons.None);
                        else Reputation.Instance.AddReputation(-Reputation.CurrentRep, TransactionReasons.None);
                        Debug.Log("[MonthlyBudgets]: " + funds + "remaining, " + penalty + " reputation removed");
                    }

                    costs = CostCalculate(true);
                    if (BudgetSettings.instance.launchCostsEnabled)
                    {
                        costs += launchCosts;
                        launchCosts = 0;
                    }

                    offsetFunds = funds - costs;
                    if (offsetFunds <0) offsetFunds = 0;
                }

                float rep = Reputation.CurrentRep;
                double budget = rep * BudgetSettings.instance.multiplier - costs;
                if (researchBudget > 0)
                {
                    float rnd = (float) budget / 10000 * (researchBudget / 100);
                    ResearchAndDevelopment.Instance.AddScience(rnd, TransactionReasons.RnDs);
                    ScreenMessages.PostScreenMessage("R&D Department have provided " + Math.Round(rnd, 1) +
                                                     " science this month");
                    Debug.Log("[MonthlyBudgets]: " + Math.Round(rnd, 1) + " science awarded by R&D");
                    Reputation.Instance.AddReputation(-(Reputation.CurrentRep * (researchBudget / 100)),
                        TransactionReasons.RnDs);
                    budget -= budget * (researchBudget / 100);
                }
                budget = Math.Max(budget, costs);
                //we shouldn't take money away. If the player holds more than the budget, just don't award.
                if (budget <= offsetFunds && BudgetSettings.instance.useItOrLoseIt)
                {
                    ScreenMessages.PostScreenMessage("We can't justify extending your budget this month");
                    if (budget < costs || !BudgetSettings.instance.coverCosts)
                    {
                        if (costs > 0)
                        {
                            Funding.Instance.AddFunds(-costs, TransactionReasons.None);
                            ScreenMessages.PostScreenMessage("This month's costs total " + costs.ToString("C"));
                        }
                    }
                    else
                    {
                        ScreenMessages.PostScreenMessage("The budget will cover your costs");
                        float repLoss = costs / BudgetSettings.instance.multiplier;
                        Reputation.Instance.AddReputation(-repLoss, TransactionReasons.None);
                    }

                    Debug.Log("[MonthlyBudgets]: Budget of " + budget + " is less than available funds of " + funds);
                }
                else
                {
                    if (enableEmergencyBudget)
                    {
                        double upgradeBudgetReserved = budget * (emergencyBudgetPercentage / 100);
                        budget -= upgradeBudgetReserved;
                        emergencyBudget += upgradeBudgetReserved;
                        emergencyBudget = Math.Round(emergencyBudget, 0);
                        Debug.Log("[MonthlyBudgets]: Diverted " + emergencyBudgetPercentage +
                                  "% of budget. BPF is now: " + emergencyBudget);
                    }

                    if(BudgetSettings.instance.useItOrLoseIt) Funding.Instance.AddFunds(-funds, TransactionReasons.None);
                    Funding.Instance.AddFunds(budget, TransactionReasons.None);
                    ScreenMessages.PostScreenMessage("This month's budget is " + budget.ToString("C"));
                    Debug.Log("[MonthlyBudgets]: Budget awarded: " + budget);
                }

                lastUpdate += BudgetSettings.instance.friendlyInterval * dayLength;
                if (BudgetSettings.instance.decayEnabled)
                {
                    Reputation.Instance.AddReputation(
                        -Reputation.CurrentRep * (BudgetSettings.instance.repDecay / 100.0f), TransactionReasons.None);
                    Debug.Log("[MonthlyBudgets]: Removing " + BudgetSettings.instance.repDecay / 100 + "% Reputation");
                }

                if (!KacWrapper.AssemblyExists && BudgetSettings.instance.stopTimewarp) TimeWarp.SetRate(0, true);
                onBudgetAwarded.Fire(budget, costs);
            }
            catch
            {
                if (HighLogic.LoadedScene != GameScenes.MAINMENU)
                    Debug.Log("[MonthlyBudgets]: Problem calculating the budget");
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
            if (!BudgetSettings.instance.masterSwitch) Destroy(this);
            instance = this;
            onBudgetAwarded = new EventData<double, double>("OnBudgetAwarded");
        }

        private void Start()
        {
            KacWrapper.InitKACWrapper();
            PopulateHomeWorldData();
            _facilities = (SpaceCenterFacility[]) Enum.GetValues(typeof(SpaceCenterFacility));
            GameEvents.OnVesselRollout.Add(OnVesselRollout);
            GameEvents.onGameSceneSwitchRequested.Add(SceneSwitch);
            onBudgetAwarded.Add(BudgetAwarded);
        }

        internal void BudgetAwarded(double budget, double costs)
        {
            if (!KacWrapper.AssemblyExists || !BudgetSettings.instance.stopTimewarp) return;
            if (!KacWrapper.APIReady) return;
            KacWrapper.KAC.CreateAlarm(KacWrapper.Kacapi.AlarmTypeEnum.Raw, "Next Budget",
                lastUpdate + BudgetSettings.instance.friendlyInterval * dayLength);
        }

        private void SceneSwitch(GameEvents.FromToAction<GameScenes, GameScenes> scenes)
        {
            if (scenes.to != GameScenes.MAINMENU) return;
            lastUpdate = 0;
            emergencyBudgetPercentage = 10;
            enableEmergencyBudget = false;
            emergencyBudget = 0;
            _timeDiscrepancyLog = true;
            researchBudget = 0;
            jokeSeen = false;
            launchCosts = 0;
        }

        public void PopulateHomeWorldData()
        {
            homeWorld = FlightGlobals.GetHomeBody();
            dayLength = homeWorld.solarDayLength;
            yearLength = homeWorld.orbit.period;
        }

        private void OnVesselRollout(ShipConstruct ship)
        {
            if (!BudgetSettings.instance.launchCostsEnabled) return;
            if (ship.shipFacility == EditorFacility.VAB)
                launchCosts += BudgetSettings.instance.launchCostsVab *
                               ((int) Math.Round(
                                    ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.LaunchPad) *
                                    ScenarioUpgradeableFacilities.GetFacilityLevelCount(SpaceCenterFacility
                                        .LaunchPad)) + 1);
            else
                launchCosts += BudgetSettings.instance.launchCostsSph *
                               ((int) Math.Round(
                                    ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.Runway) *
                                    ScenarioUpgradeableFacilities.GetFacilityLevelCount(SpaceCenterFacility.Runway)) +
                                1);
        }

        [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
        private void Update()
        {
            if (HighLogic.CurrentGame == null) return;
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER) return;
            if (lastUpdate == 99999) return;
            if (emergencyBudgetPercentage < 1) emergencyBudgetPercentage = 1;
            if (emergencyBudgetPercentage > 50) emergencyBudgetPercentage = 50;
            if (researchBudget > 100) researchBudget = 100;
            if (researchBudget < 0) researchBudget = 0;
            double time = Planetarium.GetUniversalTime();
            while (lastUpdate > time)
            {
                lastUpdate -= BudgetSettings.instance.friendlyInterval * dayLength;
                if (!_timeDiscrepancyLog) continue;
                Debug.Log("[MonthlyBudgets]: Last update was in the future. Using time machine to correct");
                _timeDiscrepancyLog = false;
            }

            double timeSinceLastUpdate = time - lastUpdate;
            if (timeSinceLastUpdate >= BudgetSettings.instance.friendlyInterval * dayLength)
                Budget(timeSinceLastUpdate);
        }

        public int CostCalculate(bool log)
        {
            IEnumerable<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew;
            int budget = 0;
            foreach (ProtoCrewMember p in crew)
            {
                if (p.type == ProtoCrewMember.KerbalType.Tourist) continue;
                float level = p.experienceLevel;
                if (level == 0) level = 0.5f;
                // ReSharper disable once RedundantAssignment
                float wages = 0;
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (p.rosterStatus)
                {
                    case ProtoCrewMember.RosterStatus.Available:
                        wages = level * BudgetSettings.instance.availableWages;
                        break;
                    case ProtoCrewMember.RosterStatus.Assigned:
                        wages = level * BudgetSettings.instance.assignedWages;
                        break;
                    default:
                        continue;
                }

                budget += (int) wages;
            }

            IEnumerable<Vessel> vessels = FlightGlobals.Vessels.Where(v =>
                v.vesselType != VesselType.Debris && v.vesselType != VesselType.Flag &&
                v.vesselType != VesselType.SpaceObject && v.vesselType != VesselType.Unknown &&
                v.vesselType != VesselType.EVA);
            budget += vessels.Count() * BudgetSettings.instance.vesselCost;
            if (BudgetSettings.instance.buildingCostsEnabled) budget += GetBuildingCosts();
            if (log) Debug.Log("[MonthlyBudgets]: Expenses are " + budget);
            return budget;
        }

        private int GetBuildingCosts()
        {
            int cost = 0;
            for (int i = 0; i < _facilities.Length; i++)
            {
                SpaceCenterFacility facility = _facilities.ElementAt(i);
                if (facility == SpaceCenterFacility.LaunchPad || facility == SpaceCenterFacility.Runway) continue;
                int lvl = (int) Math.Round(ScenarioUpgradeableFacilities.GetFacilityLevel(facility) *
                                           ScenarioUpgradeableFacilities.GetFacilityLevelCount(facility)) + 1;
                cost += lvl * BudgetSettings.instance.ReturnBuildingCosts(facility);
            }

            return cost;
        }
    }
}