using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Backup_Core.Helpers;
using Cloud_Backup_Core.Models;

namespace Cloud_Backup_Core.Viewmodels
{
    internal class BackupViewModel : BaseViewModel
    {
        private readonly SqlBackupService _backupService;
        private BackupModel _backupModel;
        private string _statusMessage;
        private bool _isBackingUp;
        private string _sqlServerName;

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
            _backupService = new SqlBackupService("");
            _backupModel = new BackupModel();
            BackupCommand = new RelayCommand(execute => ExecuteBackup(), canExecute => CanExecuteBackup());
            BrowseCommand = new RelayCommand(execute => BrowseBackupFolder(), canExecute => true);
            SetScheduleCommand = new RelayCommand(execute => SetScheduleForBackup(), canExecute => true);
            SaveBackupSettingsCommand = new RelayCommand(execute => SaveBackupSettings(), canExecute => CanExecuteBackup());

            LoadSettings();
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
            
        }

        private void SetScheduleForBackup()
        {
            throw new NotImplementedException();
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
