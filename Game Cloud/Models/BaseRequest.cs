using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Cloud.Models
{
    public class BaseRequest
    {
        public string Application { get; set; }
        public string Command { get; set; }
        public string AuthenticationCode { get; set; }
    }
}
