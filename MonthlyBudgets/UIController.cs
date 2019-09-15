using System;
using System.Collections.Generic;
using KSP.UI.Screens;
using UnityEngine;

namespace MonthlyBudgets
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    internal class UiController : MonoBehaviour
    {
        private Rect _geometry = new Rect(0.5f, 0.5f, 300, 300);
        private ApplicationLauncherButton _toolbarButton;
        private PopupDialog _uiDialog;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            GameEvents.onGUIApplicationLauncherReady.Add(GuiReady);
            GameEvents.onGameSceneSwitchRequested.Add(OnGameSceneSwitchRequested);
        }

        private static string GetNextUpdate()
        {
            if (MonthlyBudgets.instance.homeWorld == null) MonthlyBudgets.instance.PopulateHomeWorldData();
            double nextUpdateRaw = MonthlyBudgets.instance.lastUpdate +
                                   BudgetSettings.instance.friendlyInterval * MonthlyBudgets.instance.dayLength;
            double nextUpdateRefine = nextUpdateRaw / MonthlyBudgets.instance.dayLength;
            int year = 1;
            int day = 1;
            while (nextUpdateRefine > MonthlyBudgets.instance.yearLength / MonthlyBudgets.instance.dayLength)
            {
                year += 1;
                nextUpdateRefine -= MonthlyBudgets.instance.yearLength / MonthlyBudgets.instance.dayLength;
            }

            day += (int) nextUpdateRefine;
            return "Next Budget Due: Y " + year + " D " + day;
        }

        private static string EstimateBudget(bool gross)
        {
            int costs = MonthlyBudgets.instance.CostCalculate(false);
            double estimatedBudget = Math.Round(Reputation.CurrentRep * BudgetSettings.instance.multiplier, 0);
            if (estimatedBudget < costs) estimatedBudget = costs*2;
            if (gross) return "Estimated Gross Budget $:" + estimatedBudget;
            double netBudget = Math.Round(estimatedBudget - costs - MonthlyBudgets.instance.launchCosts, 0);
            return "Estimated Net Budget $:" + netBudget;
        }

        //LEGACY - Keep it here so we can see what we need to add.
        private PopupDialog GenerateDialog()
        {
            List<DialogGUIBase> dialog = new List<DialogGUIBase>();
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
            {
                dialog.Add(new DialogGUILabel("MonthlyBudgets is only available in Career Games"));
            }
            else
            {
                dialog.Add(new DialogGUILabel(GetNextUpdate));
                dialog.Add(new DialogGUILabel(() => EstimateBudget(true)));
                dialog.Add(new DialogGUILabel(() => "Current Costs: $" + MonthlyBudgets.instance.CostCalculate(false)));
                if (BudgetSettings.instance.launchCostsEnabled)
                    dialog.Add(new DialogGUILabel(() =>
                        "Launch Costs: $" + MonthlyBudgets.instance.launchCosts));
                dialog.Add(new DialogGUILabel(() => EstimateBudget(false)));
                dialog.Add(new DialogGUILabel(() =>
                    "Research Budget: " + MonthlyBudgets.instance.researchBudget + "%"));
                dialog.Add(new DialogGUISlider(() => MonthlyBudgets.instance.researchBudget, 0.0f, 100.0f, true, 140.0f,
                    30.0f, newValue => { MonthlyBudgets.instance.researchBudget = newValue; }));
                dialog.Add(new DialogGUIToggle(() => MonthlyBudgets.instance.enableEmergencyBudget,
                    "Enable Big Project Fund", b => { MonthlyBudgets.instance.enableEmergencyBudget = b; }));
                dialog.Add(new DialogGUILabel(() => "Big Project Fund: $" + MonthlyBudgets.instance.emergencyBudget,
                    null));
                dialog.Add(new DialogGUILabel(() =>
                    "Big Project Funding: " + MonthlyBudgets.instance.emergencyBudgetPercentage + "%"));
                dialog.Add(new DialogGUISlider(() => MonthlyBudgets.instance.emergencyBudgetPercentage, 1.0f, 50.0f,
                    true, 140.0f, 30.0f,
                    newValue => { MonthlyBudgets.instance.emergencyBudgetPercentage = newValue; }));
                dialog.Add(new DialogGUIButton("Withdraw Funds from Big Project Fund", WithdrawFunds, false));
                dialog.Add(new DialogGUIButton("Settings", () => BudgetSettings.instance.SpawnSettingsDialog(), false));
                dialog.Add(new DialogGUIButton("Close", CloseDialog));
            }

            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("MonthlyBudgetsDialog", "", "Monthly Budgets", UISkinManager.defaultSkin,
                    _geometry, dialog.ToArray()), true, UISkinManager.defaultSkin);
        }

        private void CloseDialog()
        {
            Vector3 rt = _uiDialog.RTrf.position;
            _geometry = new Rect(rt.x / Screen.width + 0.5f, rt.y / Screen.height + 0.5f, 300, 300);
            _uiDialog.Dismiss();
        }

        private void SpawnDialog()
        {
            if (_uiDialog == null) _uiDialog = GenerateDialog();
            else CloseDialog();
        }

        private void OnGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            if (_toolbarButton == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(_toolbarButton);
            if (_uiDialog != null && data.to == GameScenes.MAINMENU) _uiDialog.Dismiss();
        }

        public void GuiReady()
        {
            if (HighLogic.LoadedScene == GameScenes.MAINMENU || HighLogic.CurrentGame.Mode != Game.Modes.CAREER) return;
            if (_toolbarButton == null)
                _toolbarButton = ApplicationLauncher.Instance.AddModApplication(SpawnDialog, SpawnDialog, null, null,
                    null, null, ApplicationLauncher.AppScenes.ALWAYS,
                    GameDatabase.Instance.GetTexture("MonthlyBudgets/Icon", false));
        }

        public void WithdrawFunds()
        {
            Funding.Instance.AddFunds(MonthlyBudgets.instance.emergencyBudget, TransactionReasons.Strategies);
            MonthlyBudgets.instance.emergencyBudget = 0;
            MonthlyBudgets.instance.enableEmergencyBudget = false;
        }

        private void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(GuiReady);
            GameEvents.onGameSceneSwitchRequested.Remove(OnGameSceneSwitchRequested);
        }
    }
}