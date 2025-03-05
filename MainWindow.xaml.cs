using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Cloud_Backup_Core.Helpers;
using Cloud_Backup_Core.Viewmodels;

namespace Cloud_Backup_Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int UPDATE_TIMER = 300;
        private NotifyIcon _notifyIcon;
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            CreateNecessaryData();
            //InitializeComponent();
            SetupTrayIcon();
            CreateTimers();

            MainViewModel vm = new MainViewModel();

            this.Loaded += MainWindow_Loaded;
            this.DataContext = vm;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }
        private void CreateNecessaryData()
        {
            
            if (!Directory.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cloud_Backup_Core", "update")))
            {
                Directory.CreateDirectory("C:\\Users\\paokf\\AppData\\Local\\Cloud_Backup_Core\\update");
                Debug.Print("%LOCALAPPDATA% OK!");
            }
        }
        private void SetupTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(System.AppDomain.CurrentDomain.BaseDirectory + @"Resources\Content\cloud_backup.ico"),
                Visible = false
            };
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        }
        private void CreateTimers()
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // Run updater
                        RunUpdater();

                        // Wait for 20 seconds
                        await Task.Delay(TimeSpan.FromSeconds(UPDATE_TIMER), token);
                    }
                    catch (TaskCanceledException)
                    {
                        // Exit the loop if task is cancelled
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Log error or handle exception
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }, token);
        }
        private void RunUpdater()
        {
            // Path to the updater executable
            string updaterPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "CloudBackupCore", "CloudUpdater.exe");

            // Check if updater exists
            if (!System.IO.File.Exists(updaterPath))
            {
                Console.WriteLine("Updater not found.");
                return;
            }

            // Start the updater as a separate process
            var processInfo = new ProcessStartInfo
            {
                FileName = updaterPath,
                UseShellExecute = true,  // Required to run as administrator
                Verb = "runas"           // Run as admin
            };

            try
            {
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start updater: {ex.Message}");
            }
        }

        // Μπορώ να χειριστώ events Loaded, Closing, Closed
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.Hide();
            _notifyIcon.Visible = true;
        }
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                _notifyIcon.Visible = true;
            }
            else if (WindowState == WindowState.Normal)
            {
                _notifyIcon.Visible = false;
            }

            base.OnStateChanged(e);
        }
        protected override void OnClosed(EventArgs e)
        {
            _notifyIcon.Dispose();
            _notifyIcon = null;
            _cts.Cancel();
            base.OnClosed(e);
        }

    }
}