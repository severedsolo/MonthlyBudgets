using System;
using UnityEngine;

namespace MonthlyBudgets
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    internal class Af : MonoBehaviour
    {
        private bool _finalWindow;
        private bool _firstWindow = true;
        private readonly GUIStyle _headlineStyle = new GUIStyle();
        private double _originalFunding;
        private bool _secondWindow;
        private bool _showGui;
        private Rect _window = new Rect(20, 100, 240, 50);

        private void Start()
        {
            if (DateTime.Today.Day == 1 && DateTime.Today.Month == 4 && !MonthlyBudgets.instance.jokeSeen &&
                HighLogic.CurrentGame.Mode == Game.Modes.CAREER) _showGui = true;
        }

        public void OnGUI()
        {
            if (_showGui)
                _window = GUILayout.Window(99124973, _window, GuiDisplay, "Crisis at the KSC!", GUILayout.Width(200));
        }

        private void GuiDisplay(int id)
        {
            if (_firstWindow)
            {
                _headlineStyle.fontSize = 24;
                _headlineStyle.normal.textColor = Color.red;
                GUILayout.Label("From the desk of Mortimer Kerman", _headlineStyle);
                GUILayout.Label("Terrible news! The KSC has been overtaken by ransomware");
                GUILayout.Label(
                    "The hackers are threatening to delete all our data unless we pay 1 million KitKoin immediately");
                GUILayout.Label("What shall we do?");
                if (GUILayout.Button("Pay them of course!"))
                {
                    _originalFunding = Funding.Instance.Funds;
                    Funding.Instance.AddFunds(-Funding.Instance.Funds, TransactionReasons.None);
                    _firstWindow = false;
                    _showGui = false;
                    Invoke(nameof(AprilFool), 30.0f);
                }

                if (GUILayout.Button("Bah! I reject their empty threats"))
                {
                    _firstWindow = false;
                    _secondWindow = true;
                    _originalFunding = Funding.Instance.Funds;
                    Funding.Instance.AddFunds(-Funding.Instance.Funds, TransactionReasons.None);
                }
            }

            if (_secondWindow)
            {
                _headlineStyle.fontSize = 24;
                _headlineStyle.normal.textColor = Color.red;
                GUILayout.Label("From the desk of Mortimer Kerman", _headlineStyle);
                GUILayout.Label("Bad news boss");
                GUILayout.Label("Looks like the hackers found their way into our banking software");
                GUILayout.Label("Perhaps using 'Jeb' as a password was a bad idea");
                GUILayout.Label("Now they have taken all our funds and we still can't get into our computer systems");
                if (GUILayout.Button("Oh dear. Perhaps I should have paid them"))
                {
                    _showGui = false;
                    _secondWindow = false;
                    Invoke(nameof(AprilFool), 30.0f);
                }
            }

            if (!_finalWindow) return;
            _headlineStyle.fontSize = 24;
            _headlineStyle.normal.textColor = Color.red;
            GUILayout.Label("From the desk of severedsolo", _headlineStyle);
            GUILayout.Label("I'm just messing with you. April Fools!");
            GUILayout.Label("Click the button below to get your funds back");
            if (!GUILayout.Button("You got me. You are so clever severedsolo")) return;
            Funding.Instance.AddFunds(_originalFunding, TransactionReasons.None);
            _showGui = false;
            MonthlyBudgets.instance.jokeSeen = true;
        }

        private void AprilFool()
        {
            _finalWindow = true;
            _showGui = true;
        }
    }
}