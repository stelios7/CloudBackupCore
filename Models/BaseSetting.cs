using Cloud_Backup_Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Backup_Core.Models
{
    public class BaseSetting : INotifyPropertyChanged
    {
        private Logger _logger;

        public Logger Logger
        {
            get => _logger ?? new Logger();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
