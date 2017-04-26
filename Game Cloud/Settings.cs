using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Game_Cloud.Models;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Web.Helpers;

namespace Game_Cloud
{
    public class Settings
    {
        #region Data
        public static Settings Current { get; set; } = new Settings();
        public string LastUser { get; set; }
        public string AuthenticationToken { get; set; }
        #endregion Data

        #region Options
        public bool CreateLocalBackups { get; set; } = true;
        public bool ShowHelpRequests { get; set; } = true;
        public double MaximumBackupSize { get; set; } = 1024;
        public double MaximumBackupCount { get; set; } = 1000;
        public bool RememberAccount { get; set; }
        #endregion Options

        public void Save()
        {
            if (Temp.Current.Uninstall)
            {
                return;
            }
            var strPath = Utilities.AppDataFolder;
            if (!Directory.Exists(strPath))
            {
                Directory.CreateDirectory(strPath);
            }
            if (AccountInfo.Current.AccountName != null)
            {
                LastUser = AccountInfo.Current.AccountName;
            }
            var globalSettings = (Settings)MemberwiseClone();
#if DEBUG
            File.WriteAllText(strPath + @"\Settings - Global.json", Json.Encode(globalSettings));
            if (AccountInfo.Current.AccountName?.Length > 0)
            {
                File.WriteAllText(strPath + @"\Settings - " + AccountInfo.Current.AccountName + ".json", Json.Encode(AccountInfo.Current.Games));
            }
#else
            File.WriteAllText(strPath + @"\Settings - Global.json", Json.Encode(globalSettings));
            if (AccountInfo.Current.AccountName?.Length > 0)
            {
                var filePath = strPath + @"\Settings - " + AccountInfo.Current.AccountName + ".json";
                File.WriteAllText(filePath, Json.Encode(AccountInfo.Current.Games));
            }
#endif
        }

        public void LoadGlobal()
        {
            var strPath = Utilities.AppDataFolder;
            if (Directory.Exists(strPath) && File.Exists(strPath + @"\Settings - Global.json"))
            {
#if DEBUG
                var globalSettings = Json.Decode<Settings>(File.ReadAllText(strPath + @"\Settings - Global.json"));
#else
                var filePath = strPath + @"\Settings - Global.json";
                var globalSettings = Json.Decode<Settings>(File.ReadAllText(filePath));
#endif
                if (globalSettings != null)
                {
                    CreateLocalBackups = globalSettings.CreateLocalBackups;
                    MaximumBackupCount = globalSettings.MaximumBackupCount;
                    MaximumBackupSize = globalSettings.MaximumBackupSize;
                    RememberAccount = globalSettings.RememberAccount;
                    LastUser = globalSettings.LastUser;
                    AuthenticationToken = globalSettings.AuthenticationToken;
                }
            }
            if (RememberAccount)
            {
                MainWindow.Current.textAccountName.Text = LastUser;
                MainWindow.Current.passPassword.Password = "**********";
            }
        }

        public void LoadUser(string UserName)
        {
            var strPath = Utilities.AppDataFolder;
            if (Directory.Exists(strPath) && File.Exists(strPath + @"\Settings - " + UserName + ".json"))
            {
#if DEBUG
                var savedInfo = Json.Decode<List<SyncedGame>>(File.ReadAllText(strPath + @"\Settings - " + UserName + ".json"));
#else
                var filePath = strPath + @"\Settings - " + AccountInfo.Current.AccountName + ".json";
                var savedInfo = Json.Decode<List<SyncedGame>>(File.ReadAllText(filePath));
#endif
                AccountInfo.Current.Games.Clear();
                AccountInfo.Current.Games.AddRange(savedInfo);
            }
        }
    }
}