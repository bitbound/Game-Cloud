using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Cloud.Models
{
    public class AuthenticationToken
    {
        public string Token { get; set; }
        public DateTime LastUsed { get; set; }
    }
}
