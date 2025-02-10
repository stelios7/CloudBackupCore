using Cloud_Backup_Core.Helpers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Threading;

namespace Cloud_Backup_Core.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindowView.xaml
    /// </summary>
    public partial class SettingsWindowView : Window    {
        public SettingsWindowView()
        {
            Debug.WriteLine("Initiating settings window");
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(50);
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                this.Close();
            };

            timer.Start();
            this.DataContext = new Viewmodels.SettingsWindowViewModel();
            //this.Closing += SettingsWindowView_closing;
        }

        private void SettingsWindowView_closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Perform actions before the window closes
            // For example, prompt the user to save changes
            MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to save changes?", "Confirmation", System.Windows.MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                // Save changes
            }
            else if (result == MessageBoxResult.No)
            {
                // Cancel the closing operation
                //e.Cancel = true;
            }
        }

    }
}
