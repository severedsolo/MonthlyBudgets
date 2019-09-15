using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Debug = UnityEngine.Debug;
// ReSharper disable All
// ReSharper disable InvalidXmlDocComment

// TODO: Change this namespace to something specific to your plugin here.
//EG:
// namespace MyPlugin_KACWrapper
namespace MonthlyBudgets_KACWrapper
{
    ///////////////////////////////////////////////////////////////////////////////////////////
    // BELOW HERE SHOULD NOT BE EDITED - this links to the loaded KAC module without requiring a Hard Dependancy
    ///////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    ///     The Wrapper class to access KAC from another plugin
    /// </summary>
    public class KacWrapper
    {
        protected static Type KACType;
        protected static Type KACAlarmType;

        protected static object actualKAC;

        /// <summary>
        ///     This is the Kerbal Alarm Clock object
        ///     SET AFTER INIT
        /// </summary>
        public static Kacapi KAC;

        /// <summary>
        ///     Whether we managed to wrap all the methods/functions from the instance.
        ///     SET AFTER INIT
        /// </summary>
        private static bool _KACWrapped;

        /// <summary>
        ///     Whether we found the KerbalAlarmClock assembly in the loadedassemblies.
        ///     SET AFTER INIT
        /// </summary>
        public static bool AssemblyExists => KACType != null;

        /// <summary>
        ///     Whether we managed to hook the running Instance from the assembly.
        ///     SET AFTER INIT
        /// </summary>
        public static bool InstanceExists => KAC != null;

        /// <summary>
        ///     Whether the object has been wrapped and the APIReady flag is set in the real KAC
        /// </summary>
        public static bool APIReady => _KACWrapped && KAC.APIReady && !NeedUpgrade;


        public static bool NeedUpgrade { get; private set; }

        /// <summary>
        ///     This method will set up the KAC object and wrap all the methods/functions
        /// </summary>
        /// <param name="Force">This option will force the Init function to rebind everything</param>
        /// <returns></returns>
        public static bool InitKACWrapper()
        {
            //if (!_KACWrapped )
            //{
            //reset the internal objects
            _KACWrapped = false;
            actualKAC = null;
            KAC = null;
            LogFormatted("Attempting to Grab KAC Types...");

            //find the base type
            AssemblyLoader.loadedAssemblies.TypeOperation(t =>
            {
                if (t.FullName == "KerbalAlarmClock.KerbalAlarmClock")
                    KACType = t;
            });

            if (KACType == null) return false;

            LogFormatted("KAC Version:{0}", KACType.Assembly.GetName().Version.ToString());
            if (KACType.Assembly.GetName().Version.CompareTo(new Version(3, 0, 0, 5)) < 0)
                //No TimeEntry or alarmchoice options = need a newer version
                NeedUpgrade = true;

            //now the Alarm Type
            KACAlarmType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "KerbalAlarmClock.KACAlarm");

            if (KACAlarmType == null) return false;

            //now grab the running instance
            LogFormatted("Got Assembly Types, grabbing Instance");

            try
            {
                actualKAC = KACType.GetField("APIInstance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            }
            catch (Exception)
            {
                NeedUpgrade = true;
                LogFormatted("No APIInstance found - most likely you have KAC v2 installed");
                //throw;
            }

            if (actualKAC == null)
            {
                LogFormatted("Failed grabbing Instance");
                return false;
            }

            //If we get this far we can set up the local object and its methods/functions
            LogFormatted("Got Instance, Creating Wrapper Objects");
            KAC = new Kacapi(actualKAC);
            //}
            _KACWrapped = true;
            return true;
        }

        /// <summary>
        ///     The Type that is an analogue of the real KAC. This lets you access all the API-able properties and Methods of the
        ///     KAC
        /// </summary>
        public class Kacapi
        {
            public enum AlarmActionEnum
            {
                [Description("Do Nothing-Delete When Past")]
                DoNothingDeleteWhenPassed,
                [Description("Do Nothing")] DoNothing,

                [Description("Message Only-No Affect on warp")]
                MessageOnly,

                [Description("Kill Warp Only-No Message")]
                KillWarpOnly,
                [Description("Kill Warp and Message")] KillWarp,

                [Description("Pause Game and Message")]
                PauseGame
            }

            public enum AlarmTypeEnum
            {
                Raw,
                Maneuver,
                ManeuverAuto,
                Apoapsis,
                Periapsis,
                AscendingNode,
                DescendingNode,
                LaunchRendevous,
                Closest,
                SOIChange,
                SOIChangeAuto,
                Transfer,
                TransferModelled,
                Distance,
                Crew,
                EarthTime,
                Contract,
                ContractAuto,
                ScienceLab
            }

            public enum TimeEntryPrecisionEnum
            {
                Seconds = 0,
                Minutes = 1,
                Hours = 2,
                Days = 3,
                Years = 4
            }

            private readonly object actualKAC;

            private readonly FieldInfo APIReadyField;

            internal Kacapi(object KAC)
            {
                //store the actual object
                actualKAC = KAC;

                //these sections get and store the reflection info and actual objects where required. Later in the properties we then read the values from the actual objects
                //for events we also add a handler
                LogFormatted("Getting APIReady Object");
                APIReadyField = KACType.GetField("APIReady", BindingFlags.Public | BindingFlags.Static);
                LogFormatted("Success: " + (APIReadyField != null));

                //WORK OUT THE STUFF WE NEED TO HOOK FOR PEOPEL HERE
                LogFormatted("Getting Alarms Object");
                AlarmsField = KACType.GetField("alarms", BindingFlags.Public | BindingFlags.Static);
                actualAlarms = AlarmsField.GetValue(actualKAC);
                LogFormatted("Success: " + (actualAlarms != null));

                //Events
                LogFormatted("Getting Alarm State Change Event");
                onAlarmStateChangedEvent =
                    KACType.GetEvent("onAlarmStateChanged", BindingFlags.Public | BindingFlags.Instance);
                LogFormatted_DebugOnly("Success: " + (onAlarmStateChangedEvent != null));
                LogFormatted_DebugOnly("Adding Handler");
                AddHandler(onAlarmStateChangedEvent, actualKAC, AlarmStateChanged);

                //Methods
                LogFormatted("Getting Create Method");
                CreateAlarmMethod = KACType.GetMethod("CreateAlarm", BindingFlags.Public | BindingFlags.Instance);
                LogFormatted_DebugOnly("Success: " + (CreateAlarmMethod != null));

                LogFormatted("Getting Delete Method");
                DeleteAlarmMethod = KACType.GetMethod("DeleteAlarm", BindingFlags.Public | BindingFlags.Instance);
                LogFormatted_DebugOnly("Success: " + (DeleteAlarmMethod != null));

                LogFormatted("Getting DrawAlarmAction");
                DrawAlarmActionChoiceMethod = KACType.GetMethod("DrawAlarmActionChoiceAPI",
                    BindingFlags.Public | BindingFlags.Instance);
                LogFormatted_DebugOnly("Success: " + (DrawAlarmActionChoiceMethod != null));

                //LogFormatted("Getting DrawTimeEntry");
                //DrawTimeEntryMethod = KACType.GetMethod("DrawTimeEntryAPI", BindingFlags.Public | BindingFlags.Instance);
                //LogFormatted_DebugOnly("Success: " + (DrawTimeEntryMethod != null).ToString());

                //Commenting out rubbish lines
                //MethodInfo[] mis = KACType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                //foreach (MethodInfo mi in mis)
                //{
                //    LogFormatted("M:{0}-{1}", mi.Name, mi.DeclaringType);
                //}
            }

            /// <summary>
            ///     Whether the APIReady flag is set in the real KAC
            /// </summary>
            public bool APIReady
            {
                get
                {
                    if (APIReadyField == null)
                        return false;

                    return (bool) APIReadyField.GetValue(null);
                }
            }

            public class KacAlarm
            {
                public enum AlarmStateEventsEnum
                {
                    Created,
                    Triggered,
                    Closed,
                    Deleted
                }

                private readonly object actualAlarm;

                private readonly FieldInfo AlarmActionField;

                private readonly FieldInfo AlarmMarginField;

                private readonly PropertyInfo AlarmTimeProperty;

                private readonly FieldInfo AlarmTypeField;

                private readonly FieldInfo IDField;

                private readonly FieldInfo NameField;

                private readonly FieldInfo NotesField;

                private readonly FieldInfo RemainingField;


                private readonly FieldInfo RepeatAlarmField;
                private readonly PropertyInfo RepeatAlarmPeriodProperty;

                private readonly FieldInfo VesselIDField;

                private readonly FieldInfo XferOriginBodyNameField;

                private readonly FieldInfo XferTargetBodyNameField;

                internal KacAlarm(object a)
                {
                    actualAlarm = a;
                    VesselIDField = KACAlarmType.GetField("VesselID");
                    IDField = KACAlarmType.GetField("ID");
                    NameField = KACAlarmType.GetField("Name");
                    NotesField = KACAlarmType.GetField("Notes");
                    AlarmTypeField = KACAlarmType.GetField("TypeOfAlarm");
                    AlarmTimeProperty = KACAlarmType.GetProperty("AlarmTimeUT");
                    AlarmMarginField = KACAlarmType.GetField("AlarmMarginSecs");
                    AlarmActionField = KACAlarmType.GetField("AlarmAction");
                    RemainingField = KACAlarmType.GetField("Remaining");

                    XferOriginBodyNameField = KACAlarmType.GetField("XferOriginBodyName");
                    //LogFormatted("XFEROrigin:{0}", XferOriginBodyNameField == null);
                    XferTargetBodyNameField = KACAlarmType.GetField("XferTargetBodyName");

                    RepeatAlarmField = KACAlarmType.GetField("RepeatAlarm");
                    RepeatAlarmPeriodProperty = KACAlarmType.GetProperty("RepeatAlarmPeriodUT");

                    //PropertyInfo[] pis = KACAlarmType.GetProperties();
                    //foreach (PropertyInfo pi in pis)
                    //{
                    //    LogFormatted("P:{0}-{1}", pi.Name, pi.DeclaringType);
                    //}
                    //FieldInfo[] fis = KACAlarmType.GetFields();
                    //foreach (FieldInfo fi in fis)
                    //{
                    //    LogFormatted("F:{0}-{1}", fi.Name, fi.DeclaringType);
                    //}
                }

                /// <summary>
                ///     Unique Identifier of the Vessel that the alarm is attached to
                /// </summary>
                public string VesselID
                {
                    get => (string) VesselIDField.GetValue(actualAlarm);
                    set => VesselIDField.SetValue(actualAlarm, value);
                }

                /// <summary>
                ///     Unique Identifier of this alarm
                /// </summary>
                public string ID => (string) IDField.GetValue(actualAlarm);

                /// <summary>
                ///     Short Text Name for the Alarm
                /// </summary>
                public string Name
                {
                    get => (string) NameField.GetValue(actualAlarm);
                    set => NameField.SetValue(actualAlarm, value);
                }

                /// <summary>
                ///     Longer Text Description for the Alarm
                /// </summary>
                public string Notes
                {
                    get => (string) NotesField.GetValue(actualAlarm);
                    set => NotesField.SetValue(actualAlarm, value);
                }

                /// <summary>
                ///     Name of the origin body for a transfer
                /// </summary>
                public string XferOriginBodyName
                {
                    get => (string) XferOriginBodyNameField.GetValue(actualAlarm);
                    set => XferOriginBodyNameField.SetValue(actualAlarm, value);
                }

                /// <summary>
                ///     Name of the destination body for a transfer
                /// </summary>
                public string XferTargetBodyName
                {
                    get => (string) XferTargetBodyNameField.GetValue(actualAlarm);
                    set => XferTargetBodyNameField.SetValue(actualAlarm, value);
                }

                /// <summary>
                ///     What type of Alarm is this - affects icon displayed and some calc options
                /// </summary>
                public AlarmTypeEnum AlarmType => (AlarmTypeEnum) AlarmTypeField.GetValue(actualAlarm);

                /// <summary>
                ///     In game UT value of the alarm
                /// </summary>
                public double AlarmTime
                {
                    get => (double) AlarmTimeProperty.GetValue(actualAlarm, null);
                    set => AlarmTimeProperty.SetValue(actualAlarm, value, null);
                }

                /// <summary>
                ///     In game seconds the alarm will fire before the event it is for
                /// </summary>
                public double AlarmMargin
                {
                    get => (double) AlarmMarginField.GetValue(actualAlarm);
                    set => AlarmMarginField.SetValue(actualAlarm, value);
                }

                /// <summary>
                ///     What should the Alarm Clock do when the alarm fires
                /// </summary>
                public AlarmActionEnum AlarmAction
                {
                    get => (AlarmActionEnum) AlarmActionField.GetValue(actualAlarm);
                    set => AlarmActionField.SetValue(actualAlarm, (int) value);
                }

                /// <summary>
                ///     How much Game time is left before the alarm fires
                /// </summary>
                public double Remaining => (double) RemainingField.GetValue(actualAlarm);

                /// <summary>
                ///     Whether the alarm will be repeated after it fires
                /// </summary>
                public bool RepeatAlarm
                {
                    get => (bool) RepeatAlarmField.GetValue(actualAlarm);
                    set => RepeatAlarmField.SetValue(actualAlarm, value);
                }

                /// <summary>
                ///     Value in Seconds after which the alarm will repeat
                /// </summary>
                public double RepeatAlarmPeriod
                {
                    get
                    {
                        try
                        {
                            return (double) RepeatAlarmPeriodProperty.GetValue(actualAlarm, null);
                        }
                        catch (Exception)
                        {
                            return 0;
                        }
                    }
                    set => RepeatAlarmPeriodProperty.SetValue(actualAlarm, value, null);
                }
            }

            public class KacAlarmList : List<KacAlarm>
            {
            }

            #region Alarms

            private readonly object actualAlarms;
            private readonly FieldInfo AlarmsField;

            /// <summary>
            ///     The list of Alarms that are currently active in game
            /// </summary>
            internal KacAlarmList Alarms => ExtractAlarmList(actualAlarms);

            /// <summary>
            ///     This converts the KACAlarmList actual object to a new List for consumption
            /// </summary>
            /// <param name="actualAlarmList"></param>
            /// <returns></returns>
            private KacAlarmList ExtractAlarmList(object actualAlarmList)
            {
                KacAlarmList ListToReturn = new KacAlarmList();
                try
                {
                    //iterate each "value" in the dictionary

                    foreach (object item in (IList) actualAlarmList)
                    {
                        KacAlarm r1 = new KacAlarm(item);
                        ListToReturn.Add(r1);
                    }
                }
                catch (Exception)
                {
                    //LogFormatted("Arrggg: {0}", ex.Message);
                    //throw ex;
                    //
                }

                return ListToReturn;
            }

            #endregion

            #region Events

            /// <summary>
            ///     Takes an EventInfo and binds a method to the event firing
            /// </summary>
            /// <param name="Event">EventInfo of the event we want to attach to</param>
            /// <param name="KACObject">actual object the eventinfo is gathered from</param>
            /// <param name="Handler">Method that we are going to hook to the event</param>
            protected void AddHandler(EventInfo Event, object KACObject, Action<object> Handler)
            {
                //build a delegate
                Delegate d = Delegate.CreateDelegate(Event.EventHandlerType, Handler.Target, Handler.Method);
                //get the Events Add method
                MethodInfo addHandler = Event.GetAddMethod();
                //and add the delegate
                addHandler.Invoke(KACObject, new object[] {d});
            }

            //the info about the event;
            private readonly EventInfo onAlarmStateChangedEvent;

            /// <summary>
            ///     Event that fires when the State of an Alarm changes
            /// </summary>
            public event AlarmStateChangedHandler onAlarmStateChanged;

            /// <summary>
            ///     Structure of the event delegeate
            /// </summary>
            /// <param name="e"></param>
            public delegate void AlarmStateChangedHandler(AlarmStateChangedEventArgs e);

            /// <summary>
            ///     This is the structure that holds the event arguments
            /// </summary>
            public class AlarmStateChangedEventArgs
            {
                /// <summary>
                ///     Alarm that has had the state change
                /// </summary>
                public KacAlarm alarm;

                /// <summary>
                ///     What the state was before the event
                /// </summary>
                public KacAlarm.AlarmStateEventsEnum eventType;

                public AlarmStateChangedEventArgs(object actualEvent, Kacapi kac)
                {
                    Type type = actualEvent.GetType();
                    alarm = new KacAlarm(type.GetField("alarm").GetValue(actualEvent));
                    eventType = (KacAlarm.AlarmStateEventsEnum) type.GetField("eventType").GetValue(actualEvent);
                }
            }


            /// <summary>
            ///     private function that grabs the actual event and fires our wrapped one
            /// </summary>
            /// <param name="actualEvent">actual event from the KAC</param>
            private void AlarmStateChanged(object actualEvent)
            {
                if (onAlarmStateChanged != null) onAlarmStateChanged(new AlarmStateChangedEventArgs(actualEvent, this));
            }

            #endregion


            #region Methods

            private readonly MethodInfo CreateAlarmMethod;

            /// <summary>
            ///     Create a new Alarm
            /// </summary>
            /// <param name="AlarmType">What type of alarm are we creating</param>
            /// <param name="Name">Name of the Alarm for the display</param>
            /// <param name="UT">Universal Time for the alarm</param>
            /// <returns>ID of the newly created alarm</returns>
            internal string CreateAlarm(AlarmTypeEnum AlarmType, string Name, double UT)
            {
                return (string) CreateAlarmMethod.Invoke(actualKAC, new object[] {(int) AlarmType, Name, UT});
            }


            private readonly MethodInfo DeleteAlarmMethod;

            /// <summary>
            ///     Delete an Alarm
            /// </summary>
            /// <param name="AlarmID">Unique ID of the alarm</param>
            /// <returns>Success of the deletion</returns>
            internal bool DeleteAlarm(string AlarmID)
            {
                return (bool) DeleteAlarmMethod.Invoke(actualKAC, new object[] {AlarmID});
            }


            private readonly MethodInfo DrawAlarmActionChoiceMethod;

            /// <summary>
            ///     Delete an Alarm
            /// </summary>
            /// <param name="AlarmID">Unique ID of the alarm</param>
            /// <returns>Success of the deletion</returns>
            internal bool DrawAlarmActionChoice(ref AlarmActionEnum Choice, string LabelText, int LabelWidth,
                int ButtonWidth)
            {
                int InValue = (int) Choice;
                int OutValue = (int) DrawAlarmActionChoiceMethod.Invoke(actualKAC,
                    new object[] {InValue, LabelText, LabelWidth, ButtonWidth});

                Choice = (AlarmActionEnum) OutValue;
                return InValue != OutValue;
            }

            //Remmed out due to it borking window layout
            //private MethodInfo DrawTimeEntryMethod;
            ///// <summary>
            ///// Delete an Alarm
            ///// </summary>
            ///// <param name="AlarmID">Unique ID of the alarm</param>
            ///// <returns>Success of the deletion</returns>

            //internal Boolean DrawTimeEntry(ref Double Time, TimeEntryPrecisionEnum Prec, String LabelText, Int32 LabelWidth)
            //{
            //    Double InValue = Time;
            //    Double OutValue = (Double)DrawTimeEntryMethod.Invoke(actualKAC, new System.Object[] { InValue, (Int32)Prec, LabelText, LabelWidth });

            //    Time = OutValue;
            //    return (InValue != OutValue);
            //}

            #endregion
        }

        #region Logging Stuff

        /// <summary>
        ///     Some Structured logging to the debug file - ONLY RUNS WHEN DLL COMPILED IN DEBUG MODE
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        [Conditional("DEBUG")]
        internal static void LogFormatted_DebugOnly(string Message, params object[] strParams)
        {
            LogFormatted(Message, strParams);
        }

        /// <summary>
        ///     Some Structured logging to the debug file
        /// </summary>
        /// <param name="Message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        internal static void LogFormatted(string Message, params object[] strParams)
        {
            Message = string.Format(Message, strParams);
            string strMessageLine = string.Format("{0},{2}-{3},{1}",
                DateTime.Now, Message, Assembly.GetExecutingAssembly().GetName().Name,
                MethodBase.GetCurrentMethod().DeclaringType.Name);
            Debug.Log(strMessageLine);
        }

        #endregion
    }
}