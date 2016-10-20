using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Cloud.Models
{
    public class ChangeItem
    {
        public string FilePath { get; set; }
        public string FileName
        {
            get
            {
                return FilePath.Split(@"\".ToCharArray()).Last();
            }
        }
        public ChangeTypes ChangeType { get; set; }
        private DateTime? timestamp;
        public DateTime? Timestamp
        {
            get
            {
                if (timestamp.HasValue)
                {
                    return timestamp.Value.ToLocalTime();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                timestamp = value;
            }
        }
        public bool IsConflict { get; set; }
        public string ConflictMessage { get; set; }
        public enum ChangeTypes
        {
            Add,
            Delete,
            Updated
        }
    }
}
