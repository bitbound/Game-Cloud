using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Cloud.Models
{
    public class Request : BaseRequest
    {
        public Request()
        {
            Application = "GameCloud";
            AccountName = AccountInfo.Current.AccountName;
            AuthenticationCode = Settings.Current.AuthenticationCode;
        }
        public string AccountName { get; set; }
        public string AccountPassword { get; set; }
        public AccountInfo AccountInfo { get; set; }
        public SyncedGame SyncedGame { get; set; }
        public List<SyncedGame> GameList { get; set; }
        public KnownGame KnownGame { get; set; }
        public Question Question { get; set; }
        public string Note { get; set; }
    }
}
