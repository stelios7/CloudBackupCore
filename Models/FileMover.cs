using Cloud_Backup_Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Backup_Core.Models
{
    public class FileMover : BaseSetting
    { 
        public void MoveFile(string sourcePath)
        {
            string parent = Directory.GetParent(sourcePath).Parent.FullName;

            string destination = Path.Combine(parent, Path.GetFileName(sourcePath));
            File.Move(sourcePath, destination);

            DeleteOldFiles(destination);
        }

        private void DeleteOldFiles(string sourcePath)
        {
            int daysOld = 7; // Files older than this will be deleted
            try
            {
                foreach (string file in Directory.GetFiles(sourcePath))
                {
                    FileInfo fileInfo = new FileInfo(file);

                    // Check if the file is older than the specified days
                    if (fileInfo.LastWriteTime < DateTime.Now.AddDays(-daysOld))
                    {
                        Logger.Log($"Deleting: {fileInfo.FullName}", true);
                        fileInfo.Delete();
                    }
                }

               Logger.Log("Cleanup completed.", true);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error: {ex.Message}", true);
            }
        }
    }
}
