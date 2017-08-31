using Game_Cloud.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Windows;
using System.Windows.Media;

namespace Game_Cloud
{
    public static class Services
    {
#if DEBUG
        public static string ServicePath = "http://localhost:58901/Services/GameCloud";
#else
        public static string ServicePath = "https://invis.me/Services/GameCloud";
#endif
        public static async Task<HttpResponseMessage> POSTContent(Request Content)
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(3);
            var response = await client.PostAsync(Services.ServicePath, new StringContent(Json.Encode(Content)));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Error " + response.StatusCode + ": " + response.ReasonPhrase);
            }
            if (await response.Content.ReadAsStringAsync() == "unauthorized")
            {
                if (MainWindow.Current.gridGames.IsVisible)
                {
                    MessageBox.Show("Your authentication token has expired, likely due to logging in from another location.  Please log in again.", "Login Expired", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    MainWindow.Current.LogOut();
                    MainWindow.Current.passPassword.Password = "";
                }
                return null;
            }
            return response;
        }
        public static async Task<HttpResponseMessage> CheckAccount(string Password)
        {
            var content = new Request()
            {
                Command = "CheckAccount"
            };
            content.AccountPassword = Password;
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> UpdatePassword(string HashedPassword)
        {
            var content = new Request()
            {
                Command = "UpdatePassword",
                Note = HashedPassword
            };
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> CreateAccount(string NewAccountName, string HashedPassword)
        {
            var content = new Request()
            {
                AccountName = NewAccountName,
                AccountPassword = HashedPassword,
                Command = "CreateAccount",
            };
            return await POSTContent(content);
        }
        public static async Task<bool?> AddGame(SyncedGame SyncedGame)
        {
            var content = new Request()
            {
                Command = "AddGameInfo",
                SyncedGame = SyncedGame
            };
            var result = await POSTContent(content);
            return result?.IsSuccessStatusCode;
        }
        public static async Task<HttpResponseMessage> DeleteAccount()
        {
            var content = new Request()
            {
                Command = "DeleteAccount",
            };
            return await POSTContent(content);
        }
        
        public static async Task<HttpResponseMessage> ImportGames(List<SyncedGame> GameList)
        {
            var content = new Request()
            {
                Command = "ImportGames",
                GameList = GameList,
            };
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> RemoveGame(SyncedGame SyncedGame)
        {
            var content = new Request()
            {
                Command = "RemoveGame",
                SyncedGame = SyncedGame
            };
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> GetAccount()
        {
            var content = new Request()
            {
                Command = "GetAccount",
            };
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> GetGame(SyncedGame SyncedGame)
        {
            var content = new Request()
            {
                Command = "GetGame",
                SyncedGame = SyncedGame
            };
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> GetFile(SyncedGame SyncedGame, string RelativePath)
        {
            var content = new Request()
            {
                Command = "GetFile",
                SyncedGame = SyncedGame,
                Note = RelativePath
            };
            return await POSTContent(content);
        }
        public static async Task<bool?> UploadFile(SyncedGame SyncedGame, string FilePath, string RelativePath)
        {
            var webClient = new WebClient();
            webClient.Headers.Add("Command", "UploadFile");
            webClient.Headers.Add("AuthenticationToken", Settings.Current.AuthenticationToken);
            webClient.Headers.Add("AccountName", AccountInfo.Current.AccountName);
            webClient.Headers.Add("GameName", SyncedGame.Name);
            webClient.Headers.Add("RelativePath", RelativePath);
            try
            {
                await webClient.UploadFileTaskAsync(ServicePath, FilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<bool?> SyncGame(SyncedGame SyncedGame)
        {
            var content = new Request()
            {
                Command = "SyncGameInfo",
                SyncedGame = SyncedGame
            };
            var result = await POSTContent(content);
            return result.IsSuccessStatusCode;
        }
        public static async Task<HttpResponseMessage> GetKnownGames()
        {
            var content = new Request()
            {
                Command = "GetKnownGames",
            };
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> RateGame(KnownGame KnownGame)
        {
            var content = new Request()
            {
                Command = "RateKnownGame",
                KnownGame = KnownGame,
            };
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> AddKnownGame(KnownGame KnownGame)
        {
            var content = new Request()
            {
                Command = "AddKnownGame",
                KnownGame = KnownGame
            };
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> GetCurrentVersion()
        {
            var content = new Request()
            {
                Command = "GetCurrentVersion",
            };
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> ChangePassword (string HashedPassword)
        {
            var content = new Request()
            {
                Command = "ChangePassword",
                Note = HashedPassword,
            };
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> UpdateRecoveryOptions()
        {
            var content = new Request()
            {
                Command = "UpdateRecoveryOptions",
                AccountInfo = AccountInfo.Current,
            };
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> RecoverPassword(string AccountName, string Method, string Answer)
        {
            var content = new Request()
            {
                Command = "RecoverPassword",
                AccountName = AccountName,
                Note = Method + "|" + Answer,
            };
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> GetHelpRequests()
        {
            var content = new Request()
            {
                Command = "GetHelpRequests"
            };
            return await POSTContent(content);
        }
        public static async Task<HttpResponseMessage> UpdateHelpRequest(Question TheQuestion)
        {
            var content = new Request()
            {
                Command = "UpdateHelpRequest",
                Question = TheQuestion
            };
            return await POSTContent(content);
        }
    }
}
