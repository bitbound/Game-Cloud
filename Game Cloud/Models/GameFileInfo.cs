using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Cloud.Models
{
    public class GameFileInfo
    {
        public string FileName { get; set; }
        public string RelativePath { get; set; }
        public DateTime? LastWriteTime { get; set; }
    }
}
