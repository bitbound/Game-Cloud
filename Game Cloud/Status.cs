using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Cloud
{
    public static class Status
    {
        public static string SyncError { get; } = "⚠";
        public static string OK { get; } = "✔";
        public static string DownloadAvailable { get; } = "☁⬇";
        public static string UploadAvailable { get; } = "☁⬆";
        public static string DownloadUploadAvailable { get; } = "☁⬇⬆";
        public static string FolderPathUnknown { get; } = "⛔";
        public static string SavePathNotFound { get; } = "❓";
        public static string Error { get; } = "✖";
    }
}
