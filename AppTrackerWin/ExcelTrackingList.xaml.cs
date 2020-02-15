using AppTrackerWin.Models;
using AppTrackerWin.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace AppTrackerWin
{
    /// <summary>
    /// Interaction logic for ExcelTrackingList.xaml
    /// </summary>
    public partial class ExcelTrackingList : UserControl
    {
        Stopwatch sw = new Stopwatch();
        Process ExcelProcess = null;
        List<TrackedWindow> listOfVisitedWindows = new List<TrackedWindow>();
        StorageHelper _storage = new StorageHelper();
        ExcelHelper _excel = new ExcelHelper();
        DateTime started = new DateTime();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, uint dwflags);
        [DllImport("user32.dll")]
        internal static extern int UnhookWinEvent(IntPtr hWinEventHook);
        internal delegate void WinEventProc(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);

        const uint WINEVENT_OUTOFCONTEXT = 0;
        const uint EVENT_SYSTEM_FOREGROUND = 3;
        private IntPtr winHook;
        private WinEventProc listener;

        public ExcelTrackingList()
        {
            InitializeComponent();
            StartListeningForWindowChanges();
            _storage.CreateDatabaseFileIfNotExists();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            labelInfo.Content = "";
            List<TrackedWindowStorage> allStoredData = new List<TrackedWindowStorage>();
            if (startDate.SelectedDate.HasValue && endDate.SelectedDate.HasValue)
            {
                allStoredData = _storage.GetAllDatabaseEntries(startDate.SelectedDate, endDate.SelectedDate);
            }
            else
            {
                allStoredData = _storage.GetAllDatabaseEntries();
            }
            var returnData = _excel.Save(allStoredData);
            if (returnData.isError)
            {
                labelInfo.Content = returnData.Message;
                labelInfo.Foreground = new SolidColorBrush(Colors.Red);
                btnOpen.Visibility = Visibility.Hidden;
            }
            else
            {
                labelInfo.Content = "Succesfully Exported";
                labelInfo.Foreground = new SolidColorBrush(Colors.Black);
                btnOpen.Visibility = Visibility.Visible;
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"apps_usage.xlsx");
        }

        /*protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }*/

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
                started = DateTime.Now;
                Console.WriteLine("Process started: " + p.MainWindowTitle + " At: " + DateTime.Now);
            }
        }

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
            if (ExcelProcess != null)
            {
                Console.WriteLine("Process stopped: " + ExcelProcess.MainWindowTitle + " At: " + DateTime.Now);

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
                TrackedWindow trackedWindow = new TrackedWindow() { Name = cleanedWindowTitle, TimeSpent = (int)(DateTime.Now - started).TotalSeconds };

                if (!trackedWindow.Name.Contains("apps_usage"))
                {
                    _storage.AddEntryToDatabase(trackedWindow);

                    bool found = false;
                    foreach (var item in listOfVisitedWindows)
                    {
                        if (item.Name == trackedWindow.Name)
                        {
                            Console.WriteLine("Adding time: " + ExcelProcess.MainWindowTitle + " time: " + trackedWindow.TimeSpent);

                            listOfVisitedWindows.Find(x => x.Name == item.Name).TimeSpent += trackedWindow.TimeSpent;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Console.WriteLine("Process not found: " + ExcelProcess.MainWindowTitle);

                        listOfVisitedWindows.Add(trackedWindow);
                    }
                    lbList.ItemsSource = listOfVisitedWindows;
                    lbList.Items.Refresh();
                }
            }
            GetForegroundProcessName();
        }
    }
}
