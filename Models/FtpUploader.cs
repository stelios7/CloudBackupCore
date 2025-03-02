using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Cloud_Backup_Core.Helpers;
using FluentFTP;

namespace Cloud_Backup_Core.Models
{
    public class FtpUploader : BaseSetting
    {
        private static FtpUploader _instance;
        private static readonly object _lock = new object();

        public static FtpUploader Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new FtpUploader();
                    }
                    return _instance;
                }
            }
        }

        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
        public string FtpServer { get; set; }
        public int FtpPort { get; set; }

        private string ups;

        public string UploadProgressString
        {
            get { return ups; }
            set { ups = value; 
                OnPropertyChanged(nameof(UploadProgressString));
            }
        }

        private double progressValue;

        public double ProgressValue
        {
            get { return progressValue; }
            set { progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }


        private double uploadProgress;
        public double UploadProgress
        {
            get { return uploadProgress; }
            set
            {
                uploadProgress = value;
                OnPropertyChanged(nameof(UploadProgress));
            }
        }

        public FtpUploader()
        {
            FtpServer = @"seldigroup.synology.me";
            FtpUsername = @"seldi_backup";
            FtpPassword = @"Backup124578s";
            FtpPort = 33792;
        }

        private readonly string remoteDirectory;

        public async Task UploadFileFtp(CancellationToken token, string? where, string? who, string? file_to_upload = null )
        {
            try
            {
                string localFilePath = file_to_upload ?? @"C:\Users\paokf\Documents\caesium-image-compressor-2.1.0-win.zip";
                string remoteFilePath = $"/CLOUDBACKUP/{where}/{who}/{Path.GetFileName(localFilePath)}";
                var isUploadCompleted = false;

                Progress<FtpProgress> progress = new Progress<FtpProgress>(p =>
                {
                    var lfp = Path.GetFileName(localFilePath);
                    if (p.Progress == 100 && !isUploadCompleted)
                    {
                        Logger.Log($"{lfp} Uploaded", true);
                        isUploadCompleted = true;
                    }
                    else
                    {
                        Logger.Log($"{lfp} progress: {p.Progress}", true);
                    }
                    ProgressValue = p.Progress;
                    UploadProgressString = $"{((int)ProgressValue)}%";
                });

                using (var client = new AsyncFtpClient(FtpServer, FtpUsername, FtpPassword, FtpPort))
                {
                    await client.Connect(token);

                    await client.UploadFile(localFilePath, remoteFilePath, FtpRemoteExists.Overwrite, true, FtpVerify.None, progress, token);

                    await client.Disconnect(token);
                }

                
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }
        }

        public async Task ReadFTP()
        {
            try
            {

                using (var client = new AsyncFtpClient(FtpServer, FtpUsername, FtpPassword, FtpPort))
                {
                    await client.Connect();

                    foreach (var item in await client.GetListing("/CLOUDBACKUP"))
                    {
                        //FtpDirectories.Add($"{item.Type} - {item.Name} | {item.FullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task CreateFTP_Directory()
        {
            Debug.WriteLine("Creating");
            try
            {
                using (var client = new AsyncFtpClient(FtpServer, FtpUsername, FtpPassword, FtpPort))
                {
                    await client.Connect();

                    string remoteDirectory = @"/CLOUDBACKUP/Sfuel/Test";

                    bool success = await client.CreateDirectory(remoteDirectory, true);

                    if (success)
                    {
                        Debug.WriteLine($"Directory {remoteDirectory} created successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


    }
}
