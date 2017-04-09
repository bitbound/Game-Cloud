using Game_Cloud.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Cloud
{
    public class Temp
    {
        public static Temp Current { get; set; } = new Temp();
        public AccountInfo RemoteAccount { get; set; } = new AccountInfo();
        public List<KnownGame> KnownGames { get; set; } = new List<KnownGame>();
        public bool Uninstall { get; set; }
        public bool BypassAnalysis { get; set; }
        public List<Question> Questions { get; set; } = new List<Question>();
    }
}
