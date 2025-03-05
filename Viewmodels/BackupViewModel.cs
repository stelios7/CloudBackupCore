using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Cloud_Backup_Core.Helpers;
using Cloud_Backup_Core.Models;

namespace Cloud_Backup_Core.Viewmodels
{
    internal class BackupViewModel : BaseViewModel
    {
        private IScheduler _scheduler;
        private readonly SqlBackupService _backupService;
        private BackupModel _backupModel;
        private string _statusMessage;
        private bool _isBackingUp;
        private string _sqlServerName;

        private string _scheduledTime = "10:00";

        public string ScheduledTime
        {
            get { return _scheduledTime; }
            set { _scheduledTime = value; 
                OnPropertyChanged(nameof(ScheduledTime));
            }
        }


        public string SqlServerName
        {
            get { return _sqlServerName; }
            set
            {
                _sqlServerName = value;
                OnPropertyChanged(nameof(SqlServerName));
            }
        }


        public RelayCommand BackupCommand { get; }
        public RelayCommand BrowseCommand { get; }
        public RelayCommand SaveBackupSettingsCommand { get; }
        public RelayCommand SetScheduleCommand { get; }

        public BackupViewModel()
        {
            _backupService = new SqlBackupService(@$"Server={Properties.Settings.Default.SQLServerInstance};Database={Properties.Settings.Default.SQLDatabaseForBackup};Trusted_Connection=True;");
            _backupModel = new BackupModel();

            BackupCommand = new RelayCommand(execute => ExecuteBackup(), canExecute => CanExecuteBackup());
            BrowseCommand = new RelayCommand(execute => BrowseBackupFolder(), canExecute => true);
            SetScheduleCommand = new RelayCommand(execute => SetScheduleForBackup(), canExecute => true);
            SaveBackupSettingsCommand = new RelayCommand(execute => SaveBackupSettings(), canExecute => CanExecuteBackup());


            InitializeScheduler();
            LoadSettings();
        }

        private async void InitializeScheduler()
        {
            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await _scheduler.Start();

            // Reschedule the job if a schedule exists
            if (!string.IsNullOrWhiteSpace(ScheduledTime))
            {
                SetScheduleForBackup();
            }
        }

        private void SaveBackupSettings()
        {
            Properties.Settings.Default.SQLDatabaseBackupPath = BackupFolder;
            Properties.Settings.Default.SQLServerInstance = SqlServerName;
            Properties.Settings.Default.SQLDatabaseForBackup = DatabaseName;
            
            //TODO: set and save schedule backup timer

            Properties.Settings.Default.Save();
        }

        private void LoadSettings()
        {
            var settings = Properties.Settings.Default;
            BackupFolder = settings.SQLDatabaseBackupPath;
            DatabaseName = settings.SQLDatabaseForBackup;
            SqlServerName = settings.SQLServerInstance;
            ScheduledTime = settings.SQLDatabaseBackupScheduleTime.ToString("HH:mm");
        }

        private async void SetScheduleForBackup()
        {
            await _scheduler.Clear(); // Clear existing jobs before rescheduling

            if (TimeSpan.TryParse(ScheduledTime, out TimeSpan scheduleTime))
            {
                var job = JobBuilder.Create<BackupJob>()
                    .WithIdentity("BackupJob")
                    .Build();

                job.JobDataMap["BackupService"] = _backupService;
                job.JobDataMap["DatabaseName"] = DatabaseName;
                job.JobDataMap["BackupFolder"] = BackupFolder;

                var trigger = TriggerBuilder.Create()
                    .WithIdentity("BackupTrigger")
                    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(scheduleTime.Hours, scheduleTime.Minutes))
                    .Build();

                await _scheduler.ScheduleJob(job, trigger);
                StatusMessage = $"Backup scheduled at {ScheduledTime} daily.";
            }
            else
            {
                StatusMessage = "Invalid time format!";
            }
        }

        public string DatabaseName
        {
            get => _backupModel.DatabaseName;
            set
            {
                _backupModel.DatabaseName = value;
                OnPropertyChanged(nameof(DatabaseName));
            }
        }

        public string BackupFolder
        {
            get => _backupModel.BackupFolder;
            set
            {
                _backupModel.BackupFolder = value; OnPropertyChanged(nameof(BackupFolder));
            }
        }


        public String StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public bool IsBackingUp
        {
            get => _isBackingUp;
            set
            {
                _isBackingUp = value;
                OnPropertyChanged(nameof(_isBackingUp));
            }
        }

        private void BrowseBackupFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BackupFolder = dialog.SelectedPath;
            }
        }

        private async void ExecuteBackup()
        {
            IsBackingUp = true;
            StatusMessage = "Backing up...";

            bool success = await _backupService.BackupDatabaseAsync(DatabaseName, BackupFolder);

            StatusMessage = success ? "Backup completed successfully!" : "Backup failed!";
            IsBackingUp = false;
        }

        private bool CanExecuteBackup()
        {
            return !IsBackingUp && !string.IsNullOrWhiteSpace(DatabaseName) && !string.IsNullOrWhiteSpace(BackupFolder) && !string.IsNullOrWhiteSpace(SqlServerName);
        }
    }
}
