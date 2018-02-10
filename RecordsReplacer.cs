using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

namespace MonthlyBudgets
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class RecordsReplacer : MonoBehaviour
    {
        public static RecordsReplacer instance;
        List<int> speedRecords = new List<int>();
        List<int> altitudeRecords = new List<int>();
        List<int> distanceRecords = new List<int>();
        public int speedRecordIndex = 0;
        public int altitudeRecordIndex = 0;
        StringBuilder message = new StringBuilder();
        MessageSystem.Message m;
        
        void Awake()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<BudgetSettings>().DisableRecords) Destroy(this);
            instance = this;
            speedRecords.Add(25);
            speedRecords.Add(80);
            speedRecords.Add(250);
            speedRecords.Add(790);
            speedRecords.Add(2000);
            altitudeRecords.Add(500);
            altitudeRecords.Add(2000);
            altitudeRecords.Add(7000);
            altitudeRecords.Add(22000);
            altitudeRecords.Add(70000);
            message.AppendLine("We've achieved the following:");
        }

        void Update()
        {
            bool recordAchieved = false;
            message.Length = 0;
            if (FlightGlobals.ActiveVessel == null) return;
            if (speedRecordIndex < 5)
            {
                if (FlightGlobals.ActiveVessel.speed > speedRecords.ElementAt(speedRecordIndex))
                {
                    Reputation.Instance.AddReputation(3, TransactionReasons.Progression);
                    message.AppendLine("Broke speed record of " + speedRecords.ElementAt(speedRecordIndex) + " m/s");
                    message.AppendLine("Awarded <color=#EEE8AA>3</color> reputation");
                    speedRecordIndex++;
                    recordAchieved = true;
                }
            }
            if (altitudeRecordIndex < 5)
            {
                if (FlightGlobals.ActiveVessel.altitude > altitudeRecords.ElementAt(altitudeRecordIndex))
                {
                    Reputation.Instance.AddReputation(3, TransactionReasons.Progression);
                    message.AppendLine("Broke altitude record of " + altitudeRecords.ElementAt(altitudeRecordIndex) + "m");
                    recordAchieved = true;
                    altitudeRecordIndex++;
                }
            }
            if (!recordAchieved) return;
            m = new MessageSystem.Message("Progression", message.ToString(), MessageSystemButton.MessageButtonColor.BLUE, MessageSystemButton.ButtonIcons.ACHIEVE);
            MessageSystem.Instance.AddMessage(m);
        }
    }
}
