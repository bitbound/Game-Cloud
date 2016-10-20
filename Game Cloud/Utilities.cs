using Game_Cloud.Models;
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
using System.Web.Security;
using System.Windows;
using System.Windows.Media;

namespace Game_Cloud
{
    static class Utilities
    {
        public static string ResolveEnvironmentVariables(string OriginalPath)
        {
            string newPath = "";
            if (OriginalPath.Contains("%steamapps%") && VMTemp.SteamAppsFolder == null)
            {
                return null;
            }
            newPath = OriginalPath.Replace("%steamapps%", VMTemp.SteamAppsFolder);
            newPath = newPath.Replace("%systemdrive%", Path.GetPathRoot(Environment.SystemDirectory));
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
            }
            if (VMTemp.SteamAppsFolder != null && OriginalPath.ToLower().Contains(VMTemp.SteamAppsFolder.ToLower()))
            {
                newPath = "%steamapps%" + OriginalPath.Remove(0, VMTemp.SteamAppsFolder.Length);
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
            var jsonDateTime = JsonHelper.Encode(DateObject);
            return JsonHelper.Decode<DateTime>(jsonDateTime);
        }
    }
}
