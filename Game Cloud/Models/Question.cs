using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Cloud.Models
{
    public class Question
    {
        public string IsAnswered { get; set; } = "No";
        public string Asker { get; set; }
        private DateTime askedOn;
        public DateTime AskedOn
        {
            get
            {
                return askedOn.ToLocalTime();
            }
            set
            {
                askedOn = value;
            }
        }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool EmailNotify { get; set; }
        private DateTime answeredOn;
        public DateTime AnsweredOn
        {
            get
            {
                return answeredOn.ToLocalTime();
            }
            set
            {
                answeredOn = value;
            }
        }
        public string Reply { get; set; }
    }
}
