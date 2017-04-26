using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.ComponentModel;

namespace Game_Cloud.Models
{
    public class AccountInfo : INotifyPropertyChanged
    {
        public static AccountInfo Current { get; set; } = new AccountInfo();
        public AccountInfo()
        {
        }
        private string accountName;
        public string AccountName
        {
            get
            {
                return accountName;
            }
            set
            {
                accountName = value;
                FirePropertyChanged("AccountName");
            }
        }

        public string Password { get; set; }
        public string TemporaryPassword { get; set; }
        public List<AuthenticationToken> AuthenticationTokens { get; set; } = new List<AuthenticationToken>();
        public bool IsEmailEnabled { get; set; }
        public string Email { get; set; }
        public bool IsQuestionEnabled { get; set; }
        public string ChallengeQuestion { get; set; }
        public string ChallengeResponse { get; set; }
        public bool IsMachineGUIDEnabled { get; set; }
        public string MachineGUID { get; set; }
        private double storageUsed;
        public double StorageUsed
        {
            get
            {
                return storageUsed;
            }
            set
            {
                storageUsed = value;
                FirePropertyChanged("StorageUsed");
                StoragePercentage = StorageUsed / StorageTotal * 100;
            }
        }
        private double storageTotal;
        public double StorageTotal
        {
            get
            {
                return storageTotal;
            }
            set
            {
                storageTotal = value;
                FirePropertyChanged("StorageTotal");
                StoragePercentage = StorageUsed / StorageTotal * 100;
            }
        }
        private double storagePercentage;
        public double StoragePercentage
        {
            get
            {
                return storagePercentage;
            }
            set
            {
                storagePercentage = value;
                FirePropertyChanged("StoragePercentage");
            }
        }
        private List<SyncedGame> games;
        public List<SyncedGame> Games
        {
            get
            {
                if (games == null)
                {
                    games = new List<SyncedGame>();
                }
                games.Sort((SyncedGame a, SyncedGame b) => a.Name.CompareTo(b.Name));
                return games;
            }
            set
            {
                value.Sort((SyncedGame a, SyncedGame b) => a.Name.CompareTo(b.Name));
                games = value;
            }
        }
        public bool ReadTutorial { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public void FirePropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
