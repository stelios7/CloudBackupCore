using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Backup_Core.Models
{
    public class FileMover
    {
        public void MoveFile(string sourcePath)
        {
            string parent = Directory.GetParent(sourcePath).Parent.FullName;

            string destination = Path.Combine(parent, Path.GetFileName(sourcePath));
            File.Move(sourcePath, destination);
        }
    }
}
