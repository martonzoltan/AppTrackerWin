using AppTrackerWin.Models;
using AppTrackerWin.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;


namespace AppTrackerWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Stopwatch sw = new Stopwatch();
        Process ExcelProcess = null;
        List<TrackedWindow> listOfVisitedWindows = new List<TrackedWindow>();
        StorageHelper _storage = new StorageHelper();

        public MainWindow()
        {
            InitializeComponent();
            StartListeningForWindowChanges();
            _storage.CreateDatabaseFileIfNotExists();

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("Main.ico");
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // The GetWindowThreadProcessId function retrieves the identifier of the thread
        // that created the specified window and, optionally, the identifier of the
        // process that created the window.
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // Returns the name of the process owning the foreground window.
        private void GetForegroundProcessName()
        {
            IntPtr hwnd = GetForegroundWindow();

            // The foreground window can be NULL in certain circumstances, 
            // such as when a window is losing activation.
            if (hwnd == null) { }

            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);

            Process p = Process.GetProcessById((int)pid);
            if (p.ProcessName.ToLower() != "excel")
            {
                ExcelProcess = null;
            }
            else
            {
                sw.Reset();
                sw.Start();
                ExcelProcess = p;
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, uint dwflags);
        [DllImport("user32.dll")]
        internal static extern int UnhookWinEvent(IntPtr hWinEventHook);
        internal delegate void WinEventProc(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);

        const uint WINEVENT_OUTOFCONTEXT = 0;
        const uint EVENT_SYSTEM_FOREGROUND = 3;
        private IntPtr winHook;
        private WinEventProc listener;

        public void StartListeningForWindowChanges()
        {
            listener = new WinEventProc(EventCallback);
            //setting the window hook
            winHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, listener, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        public void StopListeningForWindowChanges()
        {
            UnhookWinEvent(winHook);
        }

        private void EventCallback(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {  
            if(ExcelProcess!=null)
            {
                sw.Stop();
                var cleanedWindowTitle = ExcelProcess.MainWindowTitle;
                if (cleanedWindowTitle != "")
                {
                    cleanedWindowTitle = cleanedWindowTitle.Replace("Microsoft Excel - ", "");
                    cleanedWindowTitle = cleanedWindowTitle.Replace(" - Excel", "");
                }
                else
                {
                    cleanedWindowTitle = ExcelProcess.ProcessName + " Started at: " + ExcelProcess.StartTime;
                }
                TrackedWindow trackedWindow = new TrackedWindow() { Name = cleanedWindowTitle, TimeSpent = sw.Elapsed.Seconds };
                _storage.AddEntryToDatabase(trackedWindow);

                bool found = false;
                foreach(var item in listOfVisitedWindows)
                {
                    if(item.Name == trackedWindow.Name)
                    {
                        listOfVisitedWindows.Find(x => x.Name == item.Name).TimeSpent += trackedWindow.TimeSpent;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    listOfVisitedWindows.Add(trackedWindow);
                }
                lbList.ItemsSource = listOfVisitedWindows;
                lbList.Items.Refresh();

            }
            GetForegroundProcessName(); 
        }
    }
}
