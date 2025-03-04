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

        public RelayCommand BackupCommand { get; }
        public RelayCommand BrowseCommand { get; }

        public BackupViewModel()
        {
            _backupService = new SqlBackupService("");
            _backupModel = new BackupModel();
            BackupCommand = new RelayCommand(execute => ExecuteBackup(), canExecute => CanExecuteBackup());
            BrowseCommand = new RelayCommand(execute => BrowseBackupFolder(), canExecute => true);
        }

        public string DatabaseName
        {
            get => _backupModel.DatabaseName;
            set { _backupModel.DatabaseName = value;
            OnPropertyChanged(nameof(DatabaseName));}
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
            set { _statusMessage = value;
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
            return !IsBackingUp && !string.IsNullOrWhiteSpace(DatabaseName) && !string.IsNullOrWhiteSpace(BackupFolder);
        }
    }
}
