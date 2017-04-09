using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Cloud.Models
{
    public class KnownGame
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Platform { get; set; }
        public string FileFilterOperator { get; set; }
        public string FileFilterPattern { get; set; }
        public string ID
        {
            get
            {
                return Name + Platform + Path;
            }
        }
        public List<string> PositiveRatings { get; set; } = new List<string>();
        public List<string> NegativeRatings { get; set; } = new List<string>();

        public double TotalVotes
        {
            get
            {
                return PositiveRatings.Count + NegativeRatings.Count;
            }
        }
        public double OverallRating
        {
            get
            {
                return PositiveRatings.Count - NegativeRatings.Count;
            }
        }

        public override string ToString()
        {
            return Name + " (" + Platform + ")";
        }
    }
}
