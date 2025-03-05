using Cloud_Backup_Core.Helpers;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using System.Text;
using System.Windows.Controls;
using Cloud_Backup_Core.Views;
using Cloud_Backup_Core.Models;

namespace Cloud_Backup_Core.Viewmodels
{
    internal class MainViewModel : BaseViewModel
    {
        #region RELAY COMMANDS

        //public RelayCommand UploadFile_command => new RelayCommand(async execute => await FtpUploader.UploadFileFtp(), canExecute => true);
        public RelayCommand ShowBackup_command => new RelayCommand(execute => ShowBackupForm(), canExecute => true);

        private void ShowBackupForm()
        {
            BackupViewModel bvm = new BackupViewModel();
            BackupManagerView backupManagerView = new BackupManagerView();
            backupManagerView.DataContext = bvm;
            backupManagerView.ShowDialog();
            
        }

        public RelayCommand EnterPressed_command => new RelayCommand(execute => EnterPressed(), canExecute => true);
        public RelayCommand ForceUpload_command => new RelayCommand(execute => ForceUpload(), canExecute => true);
        public RelayCommand StartSync_command => new RelayCommand(execute => StartSync(), canExecute => true);
        public RelayCommand PauseSync_command => new RelayCommand(execute => PauseSync(), canExecute => true);

        #endregion

        public FtpUploader FtpManager { get; }
        private List<CancellationTokenSource> UploadTokens { get; set; }
        public MainViewModel()
        {
            FtpManager = FtpUploader.Instance;
            BackupStatus = BACKUP_STATUS.IDLE;
            UploadTokens = new List<CancellationTokenSource>();

            BackupTimers = new List<DispatcherTimer>();
            RootDirectory = @"C:\Users\paokf\Documents\root_upload";

            SyncNow();
        }

        #region PROPERTIES DECLARATIONS

        private string fbu;

        public string FileBeingUploaded
        {
            get { return fbu; }
            set { fbu = value; 
                OnPropertyChanged(nameof(FileBeingUploaded));
            }
        }


        private string lastBackupTime;
        public string LastBackupTime
        {
            get { return lastBackupTime; }
            set
            {
                lastBackupTime = value;
                OnPropertyChanged(nameof(LastBackupTime));
            }
        }


        private BACKUP_STATUS backupStatus;
        public BACKUP_STATUS BackupStatus
        {
            get { return backupStatus; }
            set
            {
                backupStatus = value;
                OnPropertyChanged(nameof(BackupStatus));
            }
        }

        private List<DispatcherTimer> BackupTimers;
        public string RootDirectory { get; set; }

        private double upv;
        public double UploadProgressValue
        {
            get { return upv; }
            set
            {
                upv = value;
                OnPropertyChanged(nameof(UploadProgressValue));
            }
        }

        private string rootPassword;
        private List<string> ToBeUploaded;

        public string RootPassword
        {
            get { return rootPassword; }
            set
            {
                rootPassword = value;
                OnPropertyChanged(RootPassword);
            }
        }
        public enum BACKUP_STATUS
        {
            IDLE,
            ONLINE,
            UPLOADING
        }

        
        #endregion
        #region FUNCTIONS

        private string SetBackupStatus(BACKUP_STATUS status) => status switch
        {
            BACKUP_STATUS.IDLE => "Idle",
            BACKUP_STATUS.ONLINE => "Online",
            _ => "Error"
        };

        private void PauseSync()
        {
            foreach (var timer in BackupTimers)
            {
                timer.Stop();
            }
            BackupTimers.Clear();
            ToBeUploaded.Clear();
            foreach (var cts in UploadTokens)
            {
                cts.Cancel();
            }
            UploadTokens.Clear();
            BackupStatus = BACKUP_STATUS.IDLE;
            Logger.Log("Backup paused", true);
        }

        private void StartSync()
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(240);
            timer.Tick += async (o, s) => await SyncNow();
            BackupTimers.Add(timer);
            timer.Start();
            BackupStatus = BACKUP_STATUS.ONLINE;

            Task.Run(() => SyncNow())
                .ContinueWith(_ => BackupStatus = BACKUP_STATUS.IDLE);
            Logger.Log("Start syncing.", true);
        }

        private async Task SyncNow()
        {
            var uSfuel = Properties.Settings.Default.UploadSfuel;
            var uLpg = Properties.Settings.Default.UploadLpg;
            var uSoftruck = Properties.Settings.Default.UploadSoftruck;
            var uUpsales = Properties.Settings.Default.UploadUpsales;

            var pSfuel = uSfuel ? Properties.Settings.Default.SfuelLocalFilepath : "";
            var pLpg = uLpg ? Properties.Settings.Default.LpgLocalFilePath : "";
            var pSoftruck = uSoftruck ? Properties.Settings.Default.SoftruckLocalFilePath : "";
            var pUpsales = uUpsales ? Properties.Settings.Default.UpsalesLocalFilePath : "";

            var user = Properties.Settings.Default.SoftwareName;
            ToBeUploaded = new List<string>();
            UserSettings us = new UserSettings(user)
            {
                UploadSettings = {
                    new UploadSetting(uSfuel, "Sfuel", pSfuel),
                    new UploadSetting(uLpg, "LpgRetail", pLpg),
                    new UploadSetting(uSoftruck, "Softruck", pSoftruck),
                    new UploadSetting(uUpsales, "Upsales", pUpsales)
                }
            };

            if (uSfuel && File.Exists(pSfuel)) ToBeUploaded.Add(pSfuel);
            if (uLpg && File.Exists(pLpg)) ToBeUploaded.Add(pLpg);
            if (uSoftruck && File.Exists(pSoftruck)) ToBeUploaded.Add(pSoftruck);
            if (uUpsales && File.Exists(pUpsales)) ToBeUploaded.Add(pUpsales);

            foreach (var item in us.UploadSettings)
            {
                if (item.IsUploadEnabled)
                {
                    var directoryName = item.SoftwareName;
                    var files = Directory.GetFiles(item.LocalPath);
                    foreach (var file in files)
                    {
                        BackupStatus = BACKUP_STATUS.UPLOADING;
                        FileBeingUploaded = Path.GetFileName(file);
                        var cts = new CancellationTokenSource();
                        UploadTokens.Add(cts);
                        await FtpManager.UploadFileFtp(cts.Token, file, item.SoftwareName, user)
                            .ContinueWith(
                                (o) =>
                                {
                                    BackupStatus = BACKUP_STATUS.IDLE;
                                    Logger.Log($"Upload successful: {item.LocalPath}", true);
                                }
                                )
                            .ContinueWith(_ => FileMover.MoveFile(file));
                    }
                }
            }
        }

        public void RootPasswordChanged(object sender, TextChangedEventArgs e)
        {
            Debug.WriteLine(e.ToString());
        }

        private void ForceUpload()
        {
            var settings = Properties.Settings.Default;
            StringBuilder s = new StringBuilder();
            s.AppendLine($"Sfuel: {settings.SfuelLocalFilepath}");
            s.AppendLine($"Lpg: {settings.LpgLocalFilePath}");
            s.AppendLine($"Softruck: {settings.SoftruckLocalFilePath}");
            s.AppendLine($"Upsales: {settings.UpsalesLocalFilePath}");

            LastBackupTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SyncNow();
            Logger.Log(s.ToString(), true);
        }

        private void EnterPressed()
        {
            if (RootPassword == "sld" || RootPassword == "SLD")
            {
                Logger.Log("Settings timer started.", true);

                SettingsWindowView settingsWindow = new SettingsWindowView();

                settingsWindow.ShowDialog();
            }
            RootPassword = "";
        }

        #endregion

    }
}
