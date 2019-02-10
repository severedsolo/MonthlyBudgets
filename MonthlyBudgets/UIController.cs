using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

namespace MonthlyBudgets
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class UIController : MonoBehaviour
    {
        ApplicationLauncherButton toolbarButton;
        PopupDialog uiDialog;
        Rect geometry = new Rect(0.5f,0.5f,300,300);

        void Awake()
        {
            DontDestroyOnLoad(this);
            GameEvents.onGUIApplicationLauncherReady.Add(GUIReady);
            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
        }

        string GetNextUpdate()
        {
            if (MonthlyBudgets.instance.HomeWorld == null) MonthlyBudgets.instance.PopulateHomeWorldData();
            double nextUpdateRaw = MonthlyBudgets.instance.lastUpdate + (BudgetSettings.instance.friendlyInterval * MonthlyBudgets.instance.dayLength);
            double nextUpdateRefine = nextUpdateRaw / MonthlyBudgets.instance.dayLength;
            int year = 1;
            int day = 1;
            while (nextUpdateRefine > MonthlyBudgets.instance.yearLength / MonthlyBudgets.instance.dayLength)
            {
                year = year + 1;
                nextUpdateRefine = nextUpdateRefine - (MonthlyBudgets.instance.yearLength / MonthlyBudgets.instance.dayLength);
            }
            day = day + (int)nextUpdateRefine;
            return "Next Budget Due: Y " + year + " D " + day;
        }

        string EstimateBudget(bool gross)
        {
            double estimatedBudget = Math.Round(Reputation.CurrentRep * BudgetSettings.instance.multiplier, 0);
            if (estimatedBudget < 0)
            {
                estimatedBudget = 0;
            }
            if (gross) return "Estimated Gross Budget $:" + estimatedBudget;
            double netBudget = Math.Round(estimatedBudget - MonthlyBudgets.instance.CostCalculate(false) - MonthlyBudgets.instance.launchCosts,0);
            if (netBudget < 0) netBudget = 0;
            return "Estimated Net Budget $:" + netBudget;

        }

        //LEGACY - Keep it here so we can see what we need to add.
        PopupDialog GenerateDialog()
        {
            List<DialogGUIBase> dialog = new List<DialogGUIBase>();
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
            {
                dialog.Add(new DialogGUILabel("MonthlyBudgets is only available in Career Games", false, false));
            }
            else
            {
                dialog.Add(new DialogGUILabel(() => GetNextUpdate(), false, false));
                dialog.Add(new DialogGUILabel(() => EstimateBudget(true), false, false));
                dialog.Add(new DialogGUILabel(delegate { return "Current Costs: $" + MonthlyBudgets.instance.CostCalculate(false); }, false, false));
                if (BudgetSettings.instance.launchCostsEnabled) dialog.Add(new DialogGUILabel(delegate { return "Launch Costs: $" + MonthlyBudgets.instance.launchCosts; }, false, false));
                dialog.Add(new DialogGUILabel(() => EstimateBudget(false), false, false));
                dialog.Add(new DialogGUILabel(delegate { return "Research Budget: " + MonthlyBudgets.instance.researchBudget + "%"; }, false, false));
                dialog.Add(new DialogGUISlider(delegate { return MonthlyBudgets.instance.researchBudget; }, 0.0f, 100.0f, true, 140.0f, 30.0f, (float newValue) => { MonthlyBudgets.instance.researchBudget = newValue; }));
                dialog.Add(new DialogGUIToggle(() => MonthlyBudgets.instance.enableEmergencyBudget, "Enable Big Project Fund", (bool b) => { MonthlyBudgets.instance.enableEmergencyBudget = b; }));
                dialog.Add(new DialogGUILabel(delegate { return "Big Project Fund: $" + MonthlyBudgets.instance.emergencyBudget; }, null, false, false));
                dialog.Add(new DialogGUILabel(delegate { return "Big Project Funding: " + MonthlyBudgets.instance.emergencyBudgetPercentage + "%"; }, false, false));
                dialog.Add(new DialogGUISlider(delegate { return MonthlyBudgets.instance.emergencyBudgetPercentage; }, 1.0f, 50.0f, true, 140.0f, 30.0f, (float newValue) => { MonthlyBudgets.instance.emergencyBudgetPercentage = newValue; }));
                dialog.Add(new DialogGUIButton("Withdraw Funds from Big Project Fund", () => WithdrawFunds(), false));
                dialog.Add(new DialogGUIButton("Settings", () => BudgetSettings.instance.SpawnSettingsDialog(), false));
                dialog.Add(new DialogGUIButton("Close", () => CloseDialog()));
            }
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("MonthlyBudgetsDialog", "", "Monthly Budgets", UISkinManager.defaultSkin, geometry, dialog.ToArray()), true, UISkinManager.defaultSkin);
        }

        void CloseDialog()
        {
            Vector3 rt = uiDialog.RTrf.position;
            geometry = new Rect(rt.x / Screen.width + 0.5f, rt.y / Screen.height + 0.5f, 300, 300);
            uiDialog.Dismiss();
        }

        private void SpawnDialog()
        {
            if (uiDialog == null) uiDialog = GenerateDialog();
            else CloseDialog();
        }

        void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            if (toolbarButton == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
            if (uiDialog != null && data.to == GameScenes.MAINMENU) uiDialog.Dismiss();
        }

        public void GUIReady()
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER || HighLogic.LoadedScene == GameScenes.MAINMENU) return;
            if (toolbarButton == null)
            {
                toolbarButton = ApplicationLauncher.Instance.AddModApplication(SpawnDialog, SpawnDialog, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, GameDatabase.Instance.GetTexture("MonthlyBudgets/Icon", false));
            }
        }

        public void WithdrawFunds()
        {
            Funding.Instance.AddFunds(MonthlyBudgets.instance.emergencyBudget, TransactionReasons.Strategies);
            MonthlyBudgets.instance.emergencyBudget = 0;
            MonthlyBudgets.instance.enableEmergencyBudget = false;
        }

        void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(GUIReady);
            GameEvents.onGameSceneSwitchRequested.Remove(onGameSceneSwitchRequested);
        }


    }
}
