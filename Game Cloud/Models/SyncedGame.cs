using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;

namespace Game_Cloud.Models
{
    public class SyncedGame
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public double StorageUse { get; set; }
        private string status;
        public string Status
        {
            get
            {
                if (Utilities.ResolveEnvironmentVariables(Path) == null)
                {
                    StatusDetails = "The folder path could not be resolved on this computer.  You may be missing software, such as Steam.";
                    return "⛔";
                }
                else if (!Directory.Exists(Utilities.ResolveEnvironmentVariables(Path)))
                {
                    StatusDetails = "The folder path doesn't exist on this computer.  Sync this game to create it.";
                    return "❓";
                }
                else if (LastLocalSync == LastServerSync && status == null)
                {
                    StatusDetails = "This game is up-to-date.";
                    return "✔";
                }
                else
                {
                    return status;
                }
            }
            set
            {
                status = value;
            }
        }
        
        public string StatusDetails { get; set; }
        private DateTime? lastLocalSync;
        public DateTime? LastLocalSync
        {
            get
            {
                if (lastLocalSync.HasValue)
                {
                    return lastLocalSync.Value.ToLocalTime();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                lastLocalSync = value;
            }
        }

        private DateTime? lastServerSync;
        public DateTime? LastServerSync
        {
            get
            {
                if (lastServerSync.HasValue)
                {
                    return lastServerSync.Value.ToLocalTime();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                lastServerSync = value;
            }
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
