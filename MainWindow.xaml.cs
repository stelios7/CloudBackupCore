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
using Cloud_Backup_Core.Viewmodels;

namespace Cloud_Backup_Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon _notifyIcon;
        public MainWindow()
        {
            InitializeComponent();
            SetupTrayIcon();

            MainViewModel vm = new MainViewModel();

            this.Loaded += MainWindow_Loaded;
            this.DataContext = vm;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.Hide();
            _notifyIcon.Visible = true;
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

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        protected override void OnClosed(EventArgs e)
        {
            _notifyIcon.Dispose();
            _notifyIcon = null;
            base.OnClosed(e);
        }

        // Μπορώ να χειριστώ events Loaded, Closing, Closed

    }
}