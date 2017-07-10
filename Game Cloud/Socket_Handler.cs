using Game_Cloud.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Game_Cloud
{
    public static class Socket_Handler
    {
        public static WebSocket Socket { get; set; }
        public static async void HandleSocket(WebSocket WebSocket)
        {
            Socket = WebSocket;
            try
            {
                ArraySegment<byte> buffer;
                WebSocketReceiveResult result;
                string trimmedString = "";
                dynamic jsonMessage = null;
                while (Socket.State == WebSocketState.Connecting || Socket.State == WebSocketState.Open)
                {
                    buffer = ClientWebSocket.CreateClientBuffer(65536, 65536);
                    result = await Socket.ReceiveAsync(buffer, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        trimmedString = Encoding.UTF8.GetString(TrimBytes(buffer.Array));
                        jsonMessage = Json.Decode(trimmedString);

                        switch ((string)jsonMessage.Type)
                        {
                            case "GetHistory":
                                MainWindow.Current.textChatConnecting.Visibility = Visibility.Collapsed;
                                var listHistory = Json.Decode(jsonMessage.History);
                                if (listHistory != null)
                                {
                                    foreach (var item in listHistory)
                                    {
                                        AddChatItem(Json.Decode(item));
                                    }
                                }
                                break;
                            case "ChatMessage":
                                AddChatItem(jsonMessage);
                                break;
                            case "FileShare":
                                AddChatItem(jsonMessage);
                                break;
                            case "UserUpdate":
                                MainWindow.Current.listChatUsers.Items.Clear();
                                var users = jsonMessage.Users as DynamicJsonArray;
                                var listUsers = users.ToList();
                                listUsers.Sort();
                                foreach (var user in listUsers)
                                {
                                    var lbi = new ListBoxItem() { Content = user };
                                    MainWindow.Current.listChatUsers.Items.Add(lbi);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MainWindow.Current.WriteToLog(ex);
                if (MainWindow.Current.gridGames.Visibility == Visibility.Visible)
                {
                    await Socket.ConnectAsync(new Uri("wss://translucency.azurewebsites.net/Services/GameCloud/Chat"), CancellationToken.None);
                }
            }

        }
        public static void AddChatItem(dynamic JsonItem)
        {
            string sender;
            SolidColorBrush fromColor;
            if (JsonItem.User == AccountInfo.Current.AccountName)
            {
                sender = "You";
                fromColor = new SolidColorBrush(Colors.ForestGreen);
            }
            else
            {
                sender = JsonItem.User;
                fromColor = new SolidColorBrush(Colors.SteelBlue);
            }
            if (JsonItem.Type == "ChatMessage")
            {
                var runFrom = new TextBlock() {
                    Text = sender + ": ",
                    FontWeight = FontWeights.Bold,
                    Foreground = fromColor,
                    TextWrapping = TextWrapping.Wrap
                };
                MainWindow.Current.textChatWindow.Inlines.Add(runFrom);
                var runMessage = new TextBlock() {
                    Text = Encoding.UTF8.GetString(Convert.FromBase64String(JsonItem.Message)),
                    TextWrapping = TextWrapping.Wrap
                };
                MainWindow.Current.textChatWindow.Inlines.Add(runMessage);
                MainWindow.Current.textChatWindow.Inlines.Add(new LineBreak());
            }
            else if (JsonItem.Type == "FileShare")
            {
                var hyper = new Hyperlink()
                {
                    NavigateUri = new Uri(JsonItem.URL),

                };
                hyper.Inlines.Add(new Run(JsonItem.URL));
                hyper.Click += (send, arg) => {
                    Process.Start(JsonItem.URL);
                };
                var runShare = new TextBlock()
                {
                    TextWrapping = TextWrapping.Wrap
                };
                runShare.Foreground = new SolidColorBrush(Colors.DarkMagenta);
                runShare.Text = JsonItem.User + " shared a file: ";
                MainWindow.Current.textChatWindow.Inlines.Add(runShare);
                MainWindow.Current.textChatWindow.Inlines.Add(hyper);
                MainWindow.Current.textChatWindow.Inlines.Add(new LineBreak());
            }
            MainWindow.Current.scrollChat.ScrollToBottom();
            if (MainWindow.Current.tabMain.SelectedIndex != 4)
            {
                MainWindow.Current.textNewChatMessage.Text = (int.Parse(MainWindow.Current.textNewChatMessage.Text) + 1).ToString();
                MainWindow.Current.borderNewChatMessage.Visibility = Visibility.Visible;
            }
        }
        // Remove trailing empty bytes in the buffer.
        public static byte[] TrimBytes(byte[] bytes)
        {
            // Loop backwards through array until the first non-zero byte is found.
            var firstZero = 0;
            for (int i = bytes.Length - 1; i >= 0; i--)
            {
                if (bytes[i] != 0)
                {
                    firstZero = i + 1;
                    break;
                }
            }
            if (firstZero == 0)
            {
                throw new Exception("Byte array is empty.");
            }
            // Return non-empty bytes.
            return bytes.Take(firstZero).ToArray();
        }
        public static async Task SocketSend(dynamic JsonRequest)
        {
            var jsonRequest = Json.Encode(JsonRequest);
            var outBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonRequest));
            await Socket.SendAsync(outBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

    }
}
