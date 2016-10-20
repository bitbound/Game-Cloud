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

namespace Game_Cloud
{
    public class VM
    {
        #region Data
        public static VM Current { get; set; } = new VM();
        public AccountInfo AccountInfo { get; set; } = new AccountInfo();
        public string LastUser { get; set; }
        public string LastPassword { get; set; }
        #endregion Data

        #region Options
        public bool CreateLocalBackups { get; set; } = true;
        public double MaximumBackupSize { get; set; } = 1024;
        public double MaximumBackupCount { get; set; } = 1000;
        public bool RememberAccount { get; set; }
        #endregion Options

        public void Save()
        {
            if (VMTemp.Current.Uninstall)
            {
                return;
            }
            var strPath = VMTemp.AppDataFolder;
            if (!Directory.Exists(strPath))
            {
                Directory.CreateDirectory(strPath);
            }
            if (AccountInfo.AccountName != null)
            {
                LastUser = AccountInfo.AccountName;
            }
            var globalSettings = (VM)MemberwiseClone();
            globalSettings.AccountInfo = null;
#if DEBUG
            File.WriteAllText(strPath + @"\Settings - Global.json", JsonHelper.Encode(globalSettings));
            if (AccountInfo.AccountName?.Length > 0)
            {
                File.WriteAllText(strPath + @"\Settings - " + AccountInfo.AccountName + ".json", JsonHelper.Encode(AccountInfo.Games));
            }
#else
            File.WriteAllText(strPath + @"\Settings - Global.json", JsonHelper.Encode(globalSettings));
            if (AccountInfo.AccountName?.Length > 0)
            {
                var filePath = strPath + @"\Settings - " + AccountInfo.AccountName + ".json";
                File.WriteAllText(filePath, JsonHelper.Encode(AccountInfo.Games));
            }
#endif
        }

        public void LoadGlobal()
        {
            var strPath = VMTemp.AppDataFolder;
            if (Directory.Exists(strPath) && File.Exists(strPath + @"\Settings - Global.json"))
            {
#if DEBUG
                var globalSettings = JsonHelper.Decode<VM>(File.ReadAllText(strPath + @"\Settings - Global.json"));
#else
                var filePath = strPath + @"\Settings - Global.json";
                var globalSettings = JsonHelper.Decode<VM>(File.ReadAllText(filePath));
#endif
                if (globalSettings != null)
                {
                    CreateLocalBackups = globalSettings.CreateLocalBackups;
                    MaximumBackupCount = globalSettings.MaximumBackupCount;
                    MaximumBackupSize = globalSettings.MaximumBackupSize;
                    RememberAccount = globalSettings.RememberAccount;
                    LastUser = globalSettings.LastUser;
                    LastPassword = globalSettings.LastPassword;
                }
            }
            if (RememberAccount)
            {
                MainWindow.Current.textAccountName.Text = LastUser;
                MainWindow.Current.passPassword.Password = LastPassword;
            }
        }

        public void LoadUser(string UserName)
        {
            var strPath = VMTemp.AppDataFolder;
            if (Directory.Exists(strPath) && File.Exists(strPath + @"\Settings - " + UserName + ".json"))
            {
#if DEBUG
                var savedInfo = JsonHelper.Decode<List<SyncedGame>>(File.ReadAllText(strPath + @"\Settings - " + UserName + ".json"));
#else
                var filePath = strPath + @"\Settings - " + VM.Current.AccountInfo.AccountName + ".json";
                var savedInfo = JsonHelper.Decode<List<SyncedGame>>(File.ReadAllText(filePath));
#endif
                AccountInfo.Games.Clear();
                AccountInfo.Games.AddRange(savedInfo);
            }
        }
    }
}