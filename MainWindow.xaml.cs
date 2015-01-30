using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//using System.Timers;
using System.Windows.Threading;

namespace CsharpSerial3 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public class taskType : IComparable<taskType> {
        public taskType() { }
        public float stimulusCount { get; set; }
        public float recordingIntervalCount { get; set; }
        public String taskName { get; set; }
        public float avgInterval { get; set; }
        public int duration { get; set; }
        public int taskID;
        public bool usedInSet;

        public int sortOrder;
        public int sortKey;

        public int CompareTo(taskType tsk) {
            return this.sortKey.CompareTo(tsk.sortKey);
            }
        }

    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            }

        //taskType[] tasks;

        private DispatcherTimer dispatcherTimer;
        int timeRemaining;

        string orderFirstISI;
        string orderFirstChangeType;
        string orderSecondChangeType;
        string orderThirdChangeType;


        static int ID_TICKS_PRAC_500 = 1;
        static int ID_TICKS_PRAC_800 = 0;
        static int ID_T1_SMS_500 = 4;
        static int ID_T1_SMS_800 = 2;

        static int ID_TICKS_ISO_T2_500 = 10;
        static int ID_TICKS_ISO_T2_800 = 9;
        static int ID_TICKS_LINEAR_500 = 21;
        static int ID_TICKS_LINEAR_800 = 20;
        static int ID_TICKS_PHASESH_500 = 23;
        static int ID_TICKS_PHASESH_800 = 22;

        static int ID_JITS_PRAC_500 = 25;
        static int ID_JITS_PRAC_800 = 24;

        static int ID_JITS_ISO_500 = 16;
        static int ID_JITS_ISO_800 = 7;
        static int ID_JITS_LINEAR_500 = 19;
        static int ID_JITS_LINEAR_800 = 18;
        static int ID_JITS_PHASESH_500 = 17;
        static int ID_JITS_PHASESH_800 = 15;

        static int ID_ISIP_500 = 5;
        static int ID_ISIP_800 = 3;

        static int ID_PATT_PRAC = 11;
        static int ID_IMPROV_METRO = 13;
        static int ID_IMPROV_MELODY = 14;

        static int ID_REMOVED = 6;
        static int ID_MELODYACC = 8;
        static int ID_PATT_REC = 12;

        List<taskType> tasks;
        //List<taskType> orderedTaskSet;

        int[] taskSetArduinoTaskID = {
                                        0,1,2,3,4,5,6,7,8,9,
                                        10,11,12,13,14,15,16,17,18,19,
                                        20,21,22,23,24,25,
                                    };

        bool[] taskSetUsedInSet = {
                                true, true, true, true, true, true,
                                false, //x
                                true, 
                                false, //iso melody
                                true, true,
                                true, //pattern practice
                                false, // pattern record
                                true, //improv metro
                                true, //improv melody
                                true, true, true, true, 
                                true, //"Linear_Jitter_5"
                                true, true, true, true, true, true
                            };


        String[] taskSetNames;


        int[] taskSetAvgInterval = {
        800,
        500,
        800,
        800,
        500,
        500,
        100000,
        800,
        500,
        800,
        500,
        500,
        500,
        500,
        500,
        800,
        500,
        500,
        800,
        500,
        800,
        500,
        800,
        500,
        800,
        500,                                   
                                };

        byte[] taskSetStimulusCount = {  //change if anything increases above 255!
        30, // "Practice 800",     //0
        40, // "Practice 500",     //1
        120, // "Paced 800",     //2
        30, // "Unpaced 800",       //3      //DIFF
        130, // "Paced 500",     //4
        40, // "Unpaced 500",       //5      //DIFF
        120, // "[sdev removed]",  //6
        120, // "Pc MultiDev 800",   //7   
        120, // "Paced: MelodyAcc",  //8
        120, // "T2: Paced 800 t2",  //9
        130, // "T2: Paced 500 t2",  //10
        80, // "Improv/patt prac",  //11     //remove?
        80, // "Improv/patt recd",  //12      //remove?
        140, // "Improv/metronome",  //13
        140, // "Improv/melodyAcc",  //14
        170, // "Ph.Jit/8"            //15
        120, // "Pc MultiDev 500",    //16
        170, // "Ph.Jit/5"            //17
        170, // LinMDv800, 18
        170, // LinMDv500, 19
        170, // LinISOv800, 20
        170, // LinISOv500, 21  
        170, //"Ph.Tic/8",    //22
        170, //"Ph.Tic/5",    //23
        15,  //  #define ID_JITS_PRAC_800  24
        15,  //#define ID_JITS_PRAC_500  25
        };

        byte[] taskSetRecordingIntervalCount = {  //change if anything increases above 255!
        30,            // "Practice 800", //0
        40,            // "Practice 500", //1
        120,           // "Paced 800",     //2    
        150, //  *DIFF*   "Unpaced 800",   //3    // 30 paced + 120 unpaced
        130,           // "Paced 500",     //4    
        160, //  *DIFF*   "Unpaced 500",   //5    // 40 paced + 120 unpaced
        120, //                                   "[sdev removed]",  //6
        120, // "Pc MultiDev 800",   //7
        120, // "Paced: MelodyAcc",  //8
        120, // "T2: Paced 800 t2",  //9
        130, // "T2: Paced 500 t2",  //10
        80,  // "Improv/patt prac",  //11
        80,  // "Improv/patt recd",  //12
        140, // "Improv/metronome",  //13
        140, // "Improv/melodyAcc"  //14
        170, // "Ph.Jit/8"          //15
        120, // "Pc MultiDev 500",    //16
        170, // "Ph.Jit/5"            //17
        170, // LinMDv800, 18
        170, // LinMDv500, 19
        170, // LinISOv800, 20
        170, // LinISOv500, 21    
        170, //"Ph.Tic/8",    //22
        170, //"Ph.Tic/5",    //23
        15,  //  #define ID_JITS_PRAC_800  24
        15,  //#define ID_JITS_PRAC_500  25
        };

        int taskDurationAdjustment;
        int[] taskDurations = {  //change if anything increases above 255!
            24, //0
            20,
            96,
            120,
            65,
            80, //5
            12000,
            94,
            60,
            96, //9
            65,
            39,
            40,
            70, //13
            80, //14
            134,
            60,
            84,
            109,  //lin 800  //18
            109,  //lin 500
            109,  //lin 800
            109,  //lin 500  //21
            136,
            85, //23
            12,
            8,  //25
        };



        bool flashToggle;

        private void dispatcherTimer_Tick(object sender, EventArgs e) {

            //TEXT
            string answer = "DONE";

            if (timeRemaining == 10) {
                playPreNotification();
                }
            else if (timeRemaining == 0) {
                playNotification();
                }

            if (timeRemaining > 0) {
                TimeSpan t = TimeSpan.FromSeconds(timeRemaining);
                answer = string.Format("{0:D1}:{1:D2}",
                                t.Minutes,
                                t.Seconds);
                }

            labelTimer.Text = answer;
            labelTimer_Copy.Text = answer;

            //COLORS
            SolidColorBrush timerColor = new SolidColorBrush();
            timerColor.Color = Colors.Gray;

            if (timeRemaining > 40) {
                //Gray
                timerColor.Color = Color.FromRgb(180, 180, 180); //0-255
                }
            else if (timeRemaining > 20) {
                timerColor.Color = Colors.Black;
                }
            else if (timeRemaining > 10) {
                //red
                timerColor.Color = Color.FromRgb(255, 0, 0);
                }

            if (timeRemaining <= 10) {
                if (flashToggle)
                    timerColor.Color = Colors.Blue;
                else
                    timerColor.Color = Colors.Red;
                flashToggle = !flashToggle;
                }

            labelTimer.Foreground = timerColor;
            labelTimer_Copy.Foreground = timerColor;


            if (timeRemaining > -1) {
                timeRemaining--;
                }            
            
            }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            //orderedTaskSet = new List<taskType>();

            flashToggle = true;
            taskDurationAdjustment = 4;
            textTimerAdjustment.Text = taskDurationAdjustment.ToString();

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

            tasks = new List<taskType>();
            taskSetNames = new String[26];
            taskSetNames[ID_TICKS_PRAC_500] = "ID_TICKS_PRAC_500";
            taskSetNames[ID_TICKS_PRAC_800] = "ID_TICKS_PRAC_800";
            taskSetNames[ID_T1_SMS_500] = "ID_T1_SMS_500";
            taskSetNames[ID_T1_SMS_800] = "ID_T1_SMS_800";

            taskSetNames[ID_TICKS_ISO_T2_500] = "ID_TICKS_ISO_T2_500";
            taskSetNames[ID_TICKS_ISO_T2_800] = "ID_TICKS_ISO_T2_800";
            taskSetNames[ID_TICKS_LINEAR_500] = "ID_TICKS_LINEAR_500";
            taskSetNames[ID_TICKS_LINEAR_800] = "ID_TICKS_LINEAR_800";
            taskSetNames[ID_TICKS_PHASESH_500] = "ID_TICKS_PHASESH_500";
            taskSetNames[ID_TICKS_PHASESH_800] = "ID_TICKS_PHASESH_800";

            taskSetNames[ID_JITS_PRAC_500] = "ID_JITS_PRAC_500";
            taskSetNames[ID_JITS_PRAC_800] = "ID_JITS_PRAC_800";

            taskSetNames[ID_JITS_ISO_500] = "ID_JITS_ISO_500";
            taskSetNames[ID_JITS_ISO_800] = "ID_JITS_ISO_800";
            taskSetNames[ID_JITS_LINEAR_500] = "ID_JITS_LINEAR_500";
            taskSetNames[ID_JITS_LINEAR_800] = "ID_JITS_LINEAR_800";
            taskSetNames[ID_JITS_PHASESH_500] = "ID_JITS_PHASESH_500";
            taskSetNames[ID_JITS_PHASESH_800] = "ID_JITS_PHASESH_800";

            taskSetNames[ID_ISIP_500] = "ID_ISIP_500";
            taskSetNames[ID_ISIP_800] = "ID_ISIP_800";

            taskSetNames[ID_PATT_PRAC] = "ID_PATT_PRAC";
            taskSetNames[ID_IMPROV_METRO] = "ID_IMPROV_METRO";
            taskSetNames[ID_IMPROV_MELODY] = "ID_IMPROV_MELODY";

            taskSetNames[ID_REMOVED] = "ID_REMOVED";
            taskSetNames[ID_MELODYACC] = "ID_MELODYACC";
            taskSetNames[ID_PATT_REC] = "ID_PATT_REC";


            for (int i = 0; i < taskSetNames.Count(); i++) {
                taskType t = new taskType();
                t.recordingIntervalCount = taskSetRecordingIntervalCount[i];
                t.stimulusCount = taskSetStimulusCount[i];
                t.taskName = taskSetNames[i];
                t.avgInterval = taskSetAvgInterval[i];
                t.duration = taskDurations[i];
                t.usedInSet = taskSetUsedInSet[i];
                t.taskID = taskSetArduinoTaskID[i];
                tasks.Add(t);
                }

            for (int i = 0; i < tasks.Count; i++) {
                if (!tasks[i].usedInSet) {
                    tasks.RemoveAt(i);
                    }
                }

            /*
            foreach (taskType t in tasks) {
                //orderedTaskSet.Add(t);
                taskList.Items.Add(t.taskName + " [" + t.taskID + "] " +
                    t.stimulusCount + ":" + t.recordingIntervalCount);
                }
            */
            }

        private void buttonGetData_Click(object sender, RoutedEventArgs e) {
            //MainWindow mw = (MainWindow)Application.Current.MainWindow;
            //mw.Background

            }

        private void Button_Click(object sender, RoutedEventArgs e) {

            }

        private void buttonDetectArduino_Click(object sender, RoutedEventArgs e) {

            }

        private void updateOrders() {
            //don't let it do this while a session is ongoing!


            //** don't let it do this before we've removed the unused tasks...


            //orderedTaskSet.Clear();
            taskList.Items.Clear();

            int wasiIncrement = 0;
            if ((bool)radioWasiFirst.IsChecked) {
                wasiIncrement = 1;
                }


            int[] sortBit_StandardOrder = new int[26];
            sortBit_StandardOrder[ID_TICKS_PRAC_500] = 1;
            sortBit_StandardOrder[ID_TICKS_PRAC_800] = 1;
            sortBit_StandardOrder[ID_T1_SMS_500] = 2;
            sortBit_StandardOrder[ID_T1_SMS_800] = 2;
            sortBit_StandardOrder[ID_TICKS_ISO_T2_500] = 3;
            sortBit_StandardOrder[ID_TICKS_ISO_T2_800] = 3;
            sortBit_StandardOrder[ID_TICKS_LINEAR_500] = 3;
            sortBit_StandardOrder[ID_TICKS_LINEAR_800] = 3;
            sortBit_StandardOrder[ID_TICKS_PHASESH_500] = 3;
            sortBit_StandardOrder[ID_TICKS_PHASESH_800] = 3;
            sortBit_StandardOrder[ID_JITS_PRAC_500] = 4;
            sortBit_StandardOrder[ID_JITS_PRAC_800] = 4;
            sortBit_StandardOrder[ID_JITS_ISO_500] = 5;
            sortBit_StandardOrder[ID_JITS_ISO_800] = 5;
            sortBit_StandardOrder[ID_JITS_LINEAR_500] = 5;
            sortBit_StandardOrder[ID_JITS_LINEAR_800] = 5;
            sortBit_StandardOrder[ID_JITS_PHASESH_500] = 5;
            sortBit_StandardOrder[ID_JITS_PHASESH_800] = 5;
            sortBit_StandardOrder[ID_ISIP_500] = 6;
            sortBit_StandardOrder[ID_ISIP_800] = 6;
            sortBit_StandardOrder[ID_PATT_PRAC] = 7;
            sortBit_StandardOrder[ID_IMPROV_METRO] = 8;
            sortBit_StandardOrder[ID_IMPROV_MELODY] = 9;

            int[] sortBit_ISI = new int[26];
            if ((bool)radio500first.IsChecked) {
                sortBit_ISI[ID_ISIP_500] = 1;
                sortBit_ISI[ID_JITS_ISO_500] = 1;
                sortBit_ISI[ID_JITS_LINEAR_500] = 1;
                sortBit_ISI[ID_JITS_PHASESH_500] = 1;
                sortBit_ISI[ID_JITS_PRAC_500] = 1;
                sortBit_ISI[ID_T1_SMS_500] = 1;
                sortBit_ISI[ID_TICKS_ISO_T2_500] = 1;
                sortBit_ISI[ID_TICKS_LINEAR_500] = 1;
                sortBit_ISI[ID_TICKS_PHASESH_500] = 1;
                sortBit_ISI[ID_TICKS_PRAC_500] = 1;
                sortBit_ISI[ID_ISIP_800] = 2;
                sortBit_ISI[ID_JITS_ISO_800] = 2;
                sortBit_ISI[ID_JITS_LINEAR_800] = 2;
                sortBit_ISI[ID_JITS_PHASESH_800] = 2;
                sortBit_ISI[ID_JITS_PRAC_800] = 2;
                sortBit_ISI[ID_T1_SMS_800] = 2;
                sortBit_ISI[ID_TICKS_ISO_T2_800] = 2;
                sortBit_ISI[ID_TICKS_LINEAR_800] = 2;
                sortBit_ISI[ID_TICKS_PHASESH_800] = 2;
                sortBit_ISI[ID_TICKS_PRAC_800] = 2;
                orderFirstISI = "500";
                }
            else {    //"800 first" selected
                sortBit_ISI[ID_ISIP_500] = 2;
                sortBit_ISI[ID_JITS_ISO_500] = 2;
                sortBit_ISI[ID_JITS_LINEAR_500] = 2;
                sortBit_ISI[ID_JITS_PHASESH_500] = 2;
                sortBit_ISI[ID_JITS_PRAC_500] = 2;
                sortBit_ISI[ID_T1_SMS_500] = 2;
                sortBit_ISI[ID_TICKS_ISO_T2_500] = 2;
                sortBit_ISI[ID_TICKS_LINEAR_500] = 2;
                sortBit_ISI[ID_TICKS_PHASESH_500] = 2;
                sortBit_ISI[ID_TICKS_PRAC_500] = 2;
                sortBit_ISI[ID_ISIP_800] = 1;
                sortBit_ISI[ID_JITS_ISO_800] = 1;
                sortBit_ISI[ID_JITS_LINEAR_800] = 1;
                sortBit_ISI[ID_JITS_PHASESH_800] = 1;
                sortBit_ISI[ID_JITS_PRAC_800] = 1;
                sortBit_ISI[ID_T1_SMS_800] = 1;
                sortBit_ISI[ID_TICKS_ISO_T2_800] = 1;
                sortBit_ISI[ID_TICKS_LINEAR_800] = 1;
                sortBit_ISI[ID_TICKS_PHASESH_800] = 1;
                sortBit_ISI[ID_TICKS_PRAC_800] = 1;
                orderFirstISI = "800";
                }

            int[] sortBit_SecondChangeType = new int[26];
            int[] sortBit_ThirdChangeType = new int[26];

            if ((bool)radioOrder_Iso_Lin_Jump.IsChecked) {
                sortBit_SecondChangeType[ID_TICKS_ISO_T2_500] = 1;
                sortBit_SecondChangeType[ID_TICKS_ISO_T2_800] = 1;
                sortBit_SecondChangeType[ID_TICKS_LINEAR_500] = 2;
                sortBit_SecondChangeType[ID_TICKS_LINEAR_800] = 2;
                sortBit_SecondChangeType[ID_TICKS_PHASESH_500] = 1;
                sortBit_SecondChangeType[ID_TICKS_PHASESH_800] = 1;
                sortBit_SecondChangeType[ID_JITS_ISO_500] = 1;
                sortBit_SecondChangeType[ID_JITS_ISO_800] = 1;
                sortBit_SecondChangeType[ID_JITS_LINEAR_500] = 2;
                sortBit_SecondChangeType[ID_JITS_LINEAR_800] = 2;
                sortBit_SecondChangeType[ID_JITS_PHASESH_500] = 1;
                sortBit_SecondChangeType[ID_JITS_PHASESH_800] = 1;
                sortBit_ThirdChangeType[ID_TICKS_ISO_T2_500] = 1;
                sortBit_ThirdChangeType[ID_TICKS_ISO_T2_800] = 1;
                sortBit_ThirdChangeType[ID_TICKS_LINEAR_500] = 1;
                sortBit_ThirdChangeType[ID_TICKS_LINEAR_800] = 1;
                sortBit_ThirdChangeType[ID_TICKS_PHASESH_500] = 2;
                sortBit_ThirdChangeType[ID_TICKS_PHASESH_800] = 2;
                sortBit_ThirdChangeType[ID_JITS_ISO_500] = 1;
                sortBit_ThirdChangeType[ID_JITS_ISO_800] = 1;
                sortBit_ThirdChangeType[ID_JITS_LINEAR_500] = 1;
                sortBit_ThirdChangeType[ID_JITS_LINEAR_800] = 1;
                sortBit_ThirdChangeType[ID_JITS_PHASESH_500] = 2;
                sortBit_ThirdChangeType[ID_JITS_PHASESH_800] = 2;
                orderFirstChangeType = "ISO";
                orderSecondChangeType = "LIN";
                orderThirdChangeType = "JUMP";
                }

            if ((bool)radioOrder_Iso_Jump_Lin.IsChecked) {
                sortBit_SecondChangeType[ID_TICKS_ISO_T2_500] = 1;
                sortBit_SecondChangeType[ID_TICKS_ISO_T2_800] = 1;
                sortBit_SecondChangeType[ID_TICKS_LINEAR_500] = 1;
                sortBit_SecondChangeType[ID_TICKS_LINEAR_800] = 1;
                sortBit_SecondChangeType[ID_TICKS_PHASESH_500] = 2;
                sortBit_SecondChangeType[ID_TICKS_PHASESH_800] = 2;
                sortBit_SecondChangeType[ID_JITS_ISO_500] = 1;
                sortBit_SecondChangeType[ID_JITS_ISO_800] = 1;
                sortBit_SecondChangeType[ID_JITS_LINEAR_500] = 1;
                sortBit_SecondChangeType[ID_JITS_LINEAR_800] = 1;
                sortBit_SecondChangeType[ID_JITS_PHASESH_500] = 2;
                sortBit_SecondChangeType[ID_JITS_PHASESH_800] = 2;
                sortBit_ThirdChangeType[ID_TICKS_ISO_T2_500] = 1;
                sortBit_ThirdChangeType[ID_TICKS_ISO_T2_800] = 1;
                sortBit_ThirdChangeType[ID_TICKS_LINEAR_500] = 2;
                sortBit_ThirdChangeType[ID_TICKS_LINEAR_800] = 2;
                sortBit_ThirdChangeType[ID_TICKS_PHASESH_500] = 1;
                sortBit_ThirdChangeType[ID_TICKS_PHASESH_800] = 1;
                sortBit_ThirdChangeType[ID_JITS_ISO_500] = 1;
                sortBit_ThirdChangeType[ID_JITS_ISO_800] = 1;
                sortBit_ThirdChangeType[ID_JITS_LINEAR_500] = 2;
                sortBit_ThirdChangeType[ID_JITS_LINEAR_800] = 2;
                sortBit_ThirdChangeType[ID_JITS_PHASESH_500] = 1;
                sortBit_ThirdChangeType[ID_JITS_PHASESH_800] = 1;
                orderFirstChangeType = "ISO";
                orderSecondChangeType = "JUMP";
                orderThirdChangeType = "LIN";
                }

            if ((bool)radioOrder_Lin_Iso_Jump.IsChecked) {
                sortBit_SecondChangeType[ID_TICKS_ISO_T2_500] = 2;
                sortBit_SecondChangeType[ID_TICKS_ISO_T2_800] = 2;
                sortBit_SecondChangeType[ID_TICKS_LINEAR_500] = 1;
                sortBit_SecondChangeType[ID_TICKS_LINEAR_800] = 1;
                sortBit_SecondChangeType[ID_TICKS_PHASESH_500] = 1;
                sortBit_SecondChangeType[ID_TICKS_PHASESH_800] = 1;
                sortBit_SecondChangeType[ID_JITS_ISO_500] = 2;
                sortBit_SecondChangeType[ID_JITS_ISO_800] = 2;
                sortBit_SecondChangeType[ID_JITS_LINEAR_500] = 1;
                sortBit_SecondChangeType[ID_JITS_LINEAR_800] = 1;
                sortBit_SecondChangeType[ID_JITS_PHASESH_500] = 1;
                sortBit_SecondChangeType[ID_JITS_PHASESH_800] = 1;
                sortBit_ThirdChangeType[ID_TICKS_ISO_T2_500] = 1;
                sortBit_ThirdChangeType[ID_TICKS_ISO_T2_800] = 1;
                sortBit_ThirdChangeType[ID_TICKS_LINEAR_500] = 1;
                sortBit_ThirdChangeType[ID_TICKS_LINEAR_800] = 1;
                sortBit_ThirdChangeType[ID_TICKS_PHASESH_500] = 2;
                sortBit_ThirdChangeType[ID_TICKS_PHASESH_800] = 2;
                sortBit_ThirdChangeType[ID_JITS_ISO_500] = 1;
                sortBit_ThirdChangeType[ID_JITS_ISO_800] = 1;
                sortBit_ThirdChangeType[ID_JITS_LINEAR_500] = 1;
                sortBit_ThirdChangeType[ID_JITS_LINEAR_800] = 1;
                sortBit_ThirdChangeType[ID_JITS_PHASESH_500] = 2;
                sortBit_ThirdChangeType[ID_JITS_PHASESH_800] = 2;
                orderFirstChangeType = "LIN";
                orderSecondChangeType = "ISO";
                orderThirdChangeType = "JUMP";
                }

            if ((bool)radioOrder_Lin_Jump_Iso.IsChecked) {
                sortBit_SecondChangeType[ID_TICKS_ISO_T2_500] = 1;
                sortBit_SecondChangeType[ID_TICKS_ISO_T2_800] = 1;
                sortBit_SecondChangeType[ID_TICKS_LINEAR_500] = 1;
                sortBit_SecondChangeType[ID_TICKS_LINEAR_800] = 1;
                sortBit_SecondChangeType[ID_TICKS_PHASESH_500] = 2;
                sortBit_SecondChangeType[ID_TICKS_PHASESH_800] = 2;
                sortBit_SecondChangeType[ID_JITS_ISO_500] = 1;
                sortBit_SecondChangeType[ID_JITS_ISO_800] = 1;
                sortBit_SecondChangeType[ID_JITS_LINEAR_500] = 1;
                sortBit_SecondChangeType[ID_JITS_LINEAR_800] = 1;
                sortBit_SecondChangeType[ID_JITS_PHASESH_500] = 2;
                sortBit_SecondChangeType[ID_JITS_PHASESH_800] = 2;
                sortBit_ThirdChangeType[ID_TICKS_ISO_T2_500] = 2;
                sortBit_ThirdChangeType[ID_TICKS_ISO_T2_800] = 2;
                sortBit_ThirdChangeType[ID_TICKS_LINEAR_500] = 1;
                sortBit_ThirdChangeType[ID_TICKS_LINEAR_800] = 1;
                sortBit_ThirdChangeType[ID_TICKS_PHASESH_500] = 1;
                sortBit_ThirdChangeType[ID_TICKS_PHASESH_800] = 1;
                sortBit_ThirdChangeType[ID_JITS_ISO_500] = 2;
                sortBit_ThirdChangeType[ID_JITS_ISO_800] = 2;
                sortBit_ThirdChangeType[ID_JITS_LINEAR_500] = 1;
                sortBit_ThirdChangeType[ID_JITS_LINEAR_800] = 1;
                sortBit_ThirdChangeType[ID_JITS_PHASESH_500] = 1;
                sortBit_ThirdChangeType[ID_JITS_PHASESH_800] = 1;
                orderFirstChangeType = "LIN";
                orderSecondChangeType = "JUMP";
                orderThirdChangeType = "ISO";
                }

            if ((bool)radioOrder_Jump_Iso_Lin.IsChecked) {
                sortBit_SecondChangeType[ID_TICKS_ISO_T2_500] = 2;
                sortBit_SecondChangeType[ID_TICKS_ISO_T2_800] = 2;
                sortBit_SecondChangeType[ID_TICKS_LINEAR_500] = 1;
                sortBit_SecondChangeType[ID_TICKS_LINEAR_800] = 1;
                sortBit_SecondChangeType[ID_TICKS_PHASESH_500] = 1;
                sortBit_SecondChangeType[ID_TICKS_PHASESH_800] = 1;
                sortBit_SecondChangeType[ID_JITS_ISO_500] = 2;
                sortBit_SecondChangeType[ID_JITS_ISO_800] = 2;
                sortBit_SecondChangeType[ID_JITS_LINEAR_500] = 1;
                sortBit_SecondChangeType[ID_JITS_LINEAR_800] = 1;
                sortBit_SecondChangeType[ID_JITS_PHASESH_500] = 1;
                sortBit_SecondChangeType[ID_JITS_PHASESH_800] = 1;
                sortBit_ThirdChangeType[ID_TICKS_ISO_T2_500] = 1;
                sortBit_ThirdChangeType[ID_TICKS_ISO_T2_800] = 1;
                sortBit_ThirdChangeType[ID_TICKS_LINEAR_500] = 2;
                sortBit_ThirdChangeType[ID_TICKS_LINEAR_800] = 2;
                sortBit_ThirdChangeType[ID_TICKS_PHASESH_500] = 1;
                sortBit_ThirdChangeType[ID_TICKS_PHASESH_800] = 1;
                sortBit_ThirdChangeType[ID_JITS_ISO_500] = 1;
                sortBit_ThirdChangeType[ID_JITS_ISO_800] = 1;
                sortBit_ThirdChangeType[ID_JITS_LINEAR_500] = 2;
                sortBit_ThirdChangeType[ID_JITS_LINEAR_800] = 2;
                sortBit_ThirdChangeType[ID_JITS_PHASESH_500] = 1;
                sortBit_ThirdChangeType[ID_JITS_PHASESH_800] = 1;
                orderFirstChangeType = "JUMP";
                orderSecondChangeType = "ISO";
                orderThirdChangeType = "LIN";
                }

            if ((bool)radioOrder_Jump_Lin_Iso.IsChecked) {
                sortBit_SecondChangeType[ID_TICKS_ISO_T2_500] = 1;
                sortBit_SecondChangeType[ID_TICKS_ISO_T2_800] = 1;
                sortBit_SecondChangeType[ID_TICKS_LINEAR_500] = 2;
                sortBit_SecondChangeType[ID_TICKS_LINEAR_800] = 2;
                sortBit_SecondChangeType[ID_TICKS_PHASESH_500] = 1;
                sortBit_SecondChangeType[ID_TICKS_PHASESH_800] = 1;
                sortBit_SecondChangeType[ID_JITS_ISO_500] = 1;
                sortBit_SecondChangeType[ID_JITS_ISO_800] = 1;
                sortBit_SecondChangeType[ID_JITS_LINEAR_500] = 2;
                sortBit_SecondChangeType[ID_JITS_LINEAR_800] = 2;
                sortBit_SecondChangeType[ID_JITS_PHASESH_500] = 1;
                sortBit_SecondChangeType[ID_JITS_PHASESH_800] = 1;
                sortBit_ThirdChangeType[ID_TICKS_ISO_T2_500] = 2;
                sortBit_ThirdChangeType[ID_TICKS_ISO_T2_800] = 2;
                sortBit_ThirdChangeType[ID_TICKS_LINEAR_500] = 1;
                sortBit_ThirdChangeType[ID_TICKS_LINEAR_800] = 1;
                sortBit_ThirdChangeType[ID_TICKS_PHASESH_500] = 1;
                sortBit_ThirdChangeType[ID_TICKS_PHASESH_800] = 1;
                sortBit_ThirdChangeType[ID_JITS_ISO_500] = 2;
                sortBit_ThirdChangeType[ID_JITS_ISO_800] = 2;
                sortBit_ThirdChangeType[ID_JITS_LINEAR_500] = 1;
                sortBit_ThirdChangeType[ID_JITS_LINEAR_800] = 1;
                sortBit_ThirdChangeType[ID_JITS_PHASESH_500] = 1;
                sortBit_ThirdChangeType[ID_JITS_PHASESH_800] = 1;
                orderFirstChangeType = "JUMP";
                orderSecondChangeType = "LIN";
                orderThirdChangeType = "ISO";
                }

            foreach (taskType t in tasks) {
                int sortKey = 0;
                sortKey += (sortBit_StandardOrder[t.taskID] * 1000);
                sortKey += (sortBit_ThirdChangeType[t.taskID] * 100);
                sortKey += (sortBit_SecondChangeType[t.taskID] * 10);
                sortKey += (sortBit_ISI[t.taskID] * 1);
                t.sortKey = sortKey;
                }

            tasks.Sort();

            foreach (taskType t in tasks) {
                //orderedTaskSet.Add(t);
                int ct = taskList.Items.Count + 1 + wasiIncrement;

                taskList.Items.Add(ct.ToString() + ".  " + t.taskName +
                    //" (" + t.sortKey + ")" +
                    "     [" + getTaskIDCommand(t) + "]");
                }

            
            }


        private void radioOrdersChanged(object sender, RoutedEventArgs e) {
            }



        private void taskList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            labelTimer.Foreground = new SolidColorBrush(Colors.Green); //unless set otherwise below

            SolidColorBrush standardBackgroundBrush = new SolidColorBrush(Color.FromRgb(215, 228, 242));
            SolidColorBrush notificationBrush1 = new SolidColorBrush(Color.FromRgb(129, 137, 145));
            SolidColorBrush notificationBrush2 = new SolidColorBrush(Color.FromRgb(129, 137, 145));

            if (taskList.SelectedIndex == 9) {
                //last ticks item before jitter demo
                gridMainForm.Background = notificationBrush1;
                }
            else if (taskList.SelectedIndex == 17) {
                //last jitter item before ISIP
                gridMainForm.Background = notificationBrush2;
                }
            else {
                //returning to normal color after warning items
                gridMainForm.Background = standardBackgroundBrush;                
                }

            // hacky hacky hack to go to the listen-only jitter item before following through to the set list.
            if (taskList.SelectedIndex == 10) {
                Clipboard.SetText("T25");
                MessageBox.Show("(1) Advancing to the jitter items. (2) Copied the 'listen only' version. Click OK when done to proceed (copy the first jitter practice item and run the timer).");
                }
                //then follow through to the next task in the order (either jits practice 500 or 800):

            if (taskList.SelectedIndex > -1) {
                taskType selectedTask = tasks[taskList.SelectedIndex];

                string toCopy = getTaskIDCommand(selectedTask);
                Clipboard.SetText(toCopy);

                labelNotify.Text = String.Concat("Copied: ", getTaskIDCommand(selectedTask), "\n  (for ", selectedTask.taskName, ")");

                //get timer stuff
                timeRemaining = selectedTask.duration + taskDurationAdjustment;
                TimeSpan t = TimeSpan.FromSeconds(timeRemaining);
                string answer = string.Format("{0:D1}:{1:D2}",
                                t.Minutes,
                                t.Seconds);

                dispatcherTimer.Start();
                }
            }




        private string getTaskIDCommand(taskType t) {
            int idOfReceivedTask = t.taskID;
            string leadingzero = "";
            if (idOfReceivedTask < 10) { leadingzero = "0"; }

            string s = "T" + leadingzero + idOfReceivedTask.ToString();
            return s;
            }

        //private void buttonUpdateTaskOrder_Click(object sender, RoutedEventArgs e) {
        //   updateOrders();
        // }

        private void buttonAdvanceTask_Click(object sender, RoutedEventArgs e) {
            if (taskList.SelectedIndex < taskList.Items.Count - 1) {
                taskList.SelectedIndex++; //this will trigger the copy operation

                taskList.Focus();
                }
            else {
                labelNotify.Text = "no next task";
                }
            }

        private void textTimerAdjustment_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                taskDurationAdjustment = int.Parse(textTimerAdjustment.Text);
                labelNotify.Text = "set adjustment to " +
                    taskDurationAdjustment.ToString() + " seconds";
                }
            catch {
                labelNotify.Text = "couldn't parse int";
                }
            }

        private void radioOrdersClicked(object sender, RoutedEventArgs e) {
            updateOrders();
            }

        private void buttonStopTimer_Click(object sender, RoutedEventArgs e) {
            dispatcherTimer.IsEnabled = !dispatcherTimer.IsEnabled;
            /*
            if (dispatcherTimer.IsEnabled)
                buttonStopTimer.Content = "Stop Timer";
            else
                buttonStopTimer.Content = "Start Timer";
             */
            }
            

        private void buttonSoundTest_Click(object sender, RoutedEventArgs e) {
            checkBoxSoundOn.IsChecked = true;
            playNotification();
            }


        private void playNotification() {
            if ((bool)checkBoxSoundOn.IsChecked) {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer();
                player.SoundLocation = @"Media\notify2.wav";
                player.Play();
                }
            }
            
        private void playPreNotification() {
            if ((bool)checkBoxSoundOn.IsChecked) {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer();
                player.SoundLocation = @"Media\prenotify.wav";
                player.Play();
                }

            }
        }
    }