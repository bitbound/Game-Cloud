using Game_Cloud.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Security;
using System.Windows;
using System.Windows.Media;

namespace Game_Cloud
{
    static class Utilities
    {
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
        public static string ResolveEnvironmentVariables(string OriginalPath)
        {
            string newPath = "";
            if (OriginalPath.Contains("%steamapps%") && Utilities.SteamAppsFolder == null)
            {
                return null;
            }
            newPath = OriginalPath.Replace("%steamapps%", Utilities.SteamAppsFolder);
            newPath = newPath.Replace("%programfiles%", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            newPath = newPath.Replace("%programfiles(x86)%", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
            if (!Directory.Exists(Path.GetDirectoryName(newPath)) && Directory.Exists(Path.GetDirectoryName(newPath.Replace("Program Files", "Program Files (x86)"))))
            {
                newPath = newPath.Replace("Program Files", "Program Files (x86)");
            }
            newPath = newPath.Replace("%systemdrive%", Path.GetPathRoot(Environment.SystemDirectory).Replace("\\", ""));
            newPath = newPath.Replace("%localappdata%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            newPath = newPath.Replace("%appdata%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            newPath = newPath.Replace("%userprofile%", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            if (newPath.Last() != '\\')
            {
                newPath += "\\";
            }
            return newPath;
        }

        public static string FormatPathWithVariables(string OriginalPath)
        {
            string newPath = OriginalPath;
            if (OriginalPath.ToLower().Contains(Path.GetPathRoot(Environment.SystemDirectory).ToLower()))
            {
                newPath = "%systemdrive%" + OriginalPath.Remove(0, 2);
                if (OriginalPath.ToLower().Contains(Path.GetPathRoot(Environment.SystemDirectory).ToLower() + "program files (x86)"))
                {
                    newPath = "%programfiles(x86)%" + OriginalPath.Remove(0, 22);
                }
                else if (OriginalPath.ToLower().Contains(Path.GetPathRoot(Environment.SystemDirectory).ToLower() + "program files"))
                {
                    newPath = "%programfiles%" + OriginalPath.Remove(0, 16);
                }
            }
            if (Utilities.SteamAppsFolder != null && OriginalPath.ToLower().Contains(Utilities.SteamAppsFolder.ToLower()))
            {
                newPath = "%steamapps%" + OriginalPath.Remove(0, Utilities.SteamAppsFolder.Length);
            }
            if (OriginalPath.ToLower().Contains(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToLower()))
            {
                newPath = "%userprofile%" + OriginalPath.Remove(0, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Length);
                if (OriginalPath.ToLower().Contains(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToLower()))
                {
                    newPath = "%appdata%" + OriginalPath.Remove(0, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Length);
                }
                if (OriginalPath.ToLower().Contains(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToLower()))
                {
                    newPath = "%localappdata%" + OriginalPath.Remove(0, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Length);
                }
            }
            
            if (newPath.Last() != '\\')
            {
                newPath += "\\";
            }
            return newPath;
        }
        
        public static async void ShowStatus(string Message, Color Color)
        {
            MainWindow.Current.textStatus.Text = Message;
            MainWindow.Current.textStatus.Foreground = new SolidColorBrush(Color);
            await Task.Delay(1);
        }

        public static DateTime RoundDateTime(DateTime DateObject)
        {
            var jsonDateTime = Json.Encode(DateObject);
            return Json.Decode<DateTime>(jsonDateTime);
        }
    }
}
