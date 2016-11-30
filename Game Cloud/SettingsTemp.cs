using Game_Cloud.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Cloud
{
    public class SettingsTemp
    {
        public static SettingsTemp Current { get; set; } = new SettingsTemp();
        public static string AppDataFolder
        {
            get
            {
#if DEBUG
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\GameCloud\Debug\";
#else
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\GameCloud\";
#endif
            }
        }
        public static string SteamAppsFolder
        {
            get
            {
                try
                {
                    if (Environment.Is64BitOperatingSystem)
                    {
                        var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
                        if (key == null)
                        {
                            return null;
                        }
                        var value = key.GetValue("InstallPath").ToString() + @"\steamapps\common\";
                        key.Close();
                        return value;
                    }
                    else
                    {
                        var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
                        if (key == null)
                        {
                            return null;
                        }
                        var value = key.GetValue("InstallPath").ToString() + @"\steamapps\common\";
                        key.Close();
                        return value;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }
        public AccountInfo RemoteAccount { get; set; } = new AccountInfo();

        public List<KnownGame> KnownGames { get; set; } = new List<KnownGame>();
        public string AuthenticationCode { get; set; }
        public bool Uninstall { get; set; }
        public bool BypassAnalysis { get; set; }
    }
}
