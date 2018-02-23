using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MonthlyBudgets
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class AF : MonoBehaviour
    {
        bool firstWindow = true;
        bool secondWindow = false;
        bool finalWindow = false;
        Rect Window = new Rect(20, 100, 240, 50);
        private GUIStyle headlineStyle = new GUIStyle();
        double originalFunding;
        bool showGUI = false;

        void Start()
        {
            if (DateTime.Today.Day == 1 && DateTime.Today.Month == 4 && !MonthlyBudgets.instance.jokeSeen) showGUI = true;
        }

        public void OnGUI()
        {
            if (showGUI)
            {
                Window = GUILayout.Window(99124973, Window, GUIDisplay, "Crisis at the KSC!", GUILayout.Width(200));
            }
        }

        private void GUIDisplay(int id)
        {
            if (firstWindow)
            {
                headlineStyle.fontSize = 24;
                headlineStyle.normal.textColor = Color.red;
                GUILayout.Label("From the desk of Mortimer Kerman", headlineStyle);
                GUILayout.Label("Terrible news! The KSC has been overtaken by ransomware");
                GUILayout.Label("The hackers are threatening to delete all our data unless we pay 1 million KitKoin immediately");
                GUILayout.Label("What shall we do?");
                if (GUILayout.Button("Pay them of course!"))
                {
                    originalFunding = Funding.Instance.Funds;
                    Funding.Instance.AddFunds(-Funding.Instance.Funds,TransactionReasons.None);
                    firstWindow = false;
                    showGUI = false;
                    Invoke("AprilFool", 5.0f);
                }
                if (GUILayout.Button("Bah! I reject their empty threats"))
                {
                    firstWindow = false;
                    secondWindow = true;
                    originalFunding = Funding.Instance.Funds;
                    Funding.Instance.AddFunds(-Funding.Instance.Funds, TransactionReasons.None);
                }
            }
            if(secondWindow)
            {
                headlineStyle.fontSize = 24;
                headlineStyle.normal.textColor = Color.red;
                GUILayout.Label("From the desk of Mortimer Kerman", headlineStyle);
                GUILayout.Label("Bad news boss");
                GUILayout.Label("Looks like the hackers found their way into our banking software");
                GUILayout.Label("Perhaps using 'Jeb' as a password was a bad idea");
                GUILayout.Label("Now they have taken all our funds and we still can't get into our computer systems");
                if (GUILayout.Button("Oh dear. Perhaps I should have paid them"))
                {
                    showGUI = false;
                    secondWindow = false;
                    Invoke("AprilFool", 5.0f);
                }
            }
            if(finalWindow)
            {
                headlineStyle.fontSize = 24;
                headlineStyle.normal.textColor = Color.red;
                GUILayout.Label("From the desk of severedsolo", headlineStyle);
                GUILayout.Label("I'm just messing with you. April Fools!");
                GUILayout.Label("Click the button below to get your funds back");
                if(GUILayout.Button("You got me. You are so clever severedsolo"))
                {
                    Funding.Instance.AddFunds(originalFunding,TransactionReasons.None);
                    showGUI = false;
                    MonthlyBudgets.instance.jokeSeen = true;
                }
            }
        }
        void AprilFool()
        {
            finalWindow = true;
            showGUI = true;
        }
    }
}
