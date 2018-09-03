using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Game_Cloud.Models;
using System.IO.Compression;
using System.Diagnostics;
using Microsoft.Win32;
using Game_Cloud.Windows;
using System.Data;
using System.Web.Helpers;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Windows.Documents;

namespace Game_Cloud
{
    public partial class MainWindow : Window
    {
        public static MainWindow Current { get; set; }
        public WebSocket Socket { get; set; }
        public MainWindow()
        {
            App.Current.DispatcherUnhandledException += DispatcherUnhandledException;
            InitializeComponent();
            this.DataContext = AccountInfo.Current;
            comboKnownGames.DataContext = Temp.Current;
            menuAccount.DataContext = Temp.Current;
            dataQuestions.DataContext = Temp.Current;
            Current = this;

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1]))
            {
                var success = false;
                var startTime = DateTime.Now;
                while (success == false)
                {
                    System.Threading.Thread.Sleep(200);
                    if (DateTime.Now - startTime > TimeSpan.FromSeconds(5))
                    {
                        break;
                    }
                    try
                    {
                        File.Copy(System.Reflection.Assembly.GetExecutingAssembly().Location, args[1], true);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        WriteToLog(ex);
                        continue;
                    }
                }
                if (success == false)
                {
                    MessageBox.Show("Update failed.  Please close all Game Cloud windows, then try again.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Update successful!  Game Cloud will now restart.", "Update Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    Process.Start(args[1]);
                }
                App.Current.Shutdown();
                return;
            }
        }

        #region Main Window
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Utilities.ShowStatus("Checking for updates...", Colors.Green);
            try
            {
                var client = new HttpClient();
                await CheckForUpdates(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The server cannot be reached.  Please check your internet connection." + Environment.NewLine + Environment.NewLine + "Error Details: " + ex.Message, "Network Unavailable", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Utilities.ShowStatus("", Colors.Green);
            Settings.Current.LoadGlobal();
            if (Settings.Current.RememberAccount && Settings.Current.LastUser?.Length > 0)
            {
                textAccountName.Text = Settings.Current.LastUser;
                passPassword.Password = "************";
                checkRememberAccount.IsChecked = Settings.Current.RememberAccount;
            }
        }
        private async void Window_Activated(object sender, EventArgs e)
        {
            if (IsActive && tabMain.Visibility == Visibility.Visible && tabMain.SelectedIndex == 0 && gridLogIn.Visibility == Visibility.Collapsed && gridNewAccount.Visibility == Visibility.Collapsed)
            {
                await AnalyzeChanges();
            }
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void hyperLogOut_Click(object sender, RoutedEventArgs e)
        {
            Settings.Current.Save();
            LogOut();
        }
        private void progressStorage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue < 50)
            {
                progressStorage.Foreground = new SolidColorBrush(Color.FromArgb(255, 6, 176, 37));
            }
            else if (e.NewValue < 75)
            {
                progressStorage.Foreground = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                progressStorage.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
        private void buttonMenu_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsOpen = true;
        }
        private void menuOptions_Click(object sender, RoutedEventArgs e)
        {
            var win = new Windows.Options();
            win.Owner = this;
            win.ShowDialog();
        }
        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {
            var win = new Windows.About();
            win.Owner = this;
            win.ShowDialog();
        }
        private void menuWelcome_Click(object sender, RoutedEventArgs e)
        {
            var welcome = new Welcome();
            welcome.Owner = this;
            welcome.ShowDialog();
        }
        private void menuAccount_Click(object sender, RoutedEventArgs e)
        {
            var win = new Windows.Account();
            win.Owner = this;
            win.ShowDialog();
        }

        private async void menuUpdates_Click(object sender, RoutedEventArgs e)
        {
            await CheckForUpdates(false);
        }


        private void hyperSignUp_Click(object sender, RoutedEventArgs e)
        {
            gridLogIn.Visibility = Visibility.Collapsed;
            gridNewAccount.Visibility = Visibility.Visible;
            textNewAccountName.Text = "";
            passwordNew.Password = "";
            passwordNewConfirm.Password = "";
            textNewAccountName.Focus();
        }
        private void DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Utilities.ShowStatus("An error has occurred.  Please send a bug report.", Colors.Red);
            tabMain.IsEnabled = true;
            WriteToLog(e.Exception);
        }
        #endregion Main Window



        #region Login
        private void textUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            buttonLogIn.IsDefault = true;
        }

        private void textUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            buttonLogIn.IsDefault = false;
        }
        private void passPassword_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Settings.Current.AuthenticationToken = null;
        }
        private async void buttonLogIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new HttpClient();
                await client.GetAsync(Services.EchoPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The server cannot be reached.  Please check your internet connection." + Environment.NewLine + Environment.NewLine + "Error Details: " + ex.Message, "Network Unavailable", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Utilities.ShowStatus("Logging in...", Colors.Green);
            buttonLogIn.IsEnabled = false;
            dataSyncedGames.Items.Refresh();
            if (textAccountName.Text.Length == 0)
            {
                Utilities.ShowStatus("Username cannot be empty.", Colors.Red);
                buttonLogIn.IsEnabled = true;
                return;
            }
            await LogIn(textAccountName.Text.Trim(), passPassword.Password);
        }
        private async void hyperForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            if (textAccountName.Text.Length == 0)
            {
                MessageBox.Show("You must enter an account name to recover your password.", "Account Name Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            var response = await Services.RecoverPassword(textAccountName.Text, null, null);
            if (response == null)
            {
                return;
            }
            var strResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                if (strResponse == "false")
                {
                    MessageBox.Show("The specified account name doesn't exist.", "Unknown Account", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                var methods = strResponse.Split('|');
                if (methods[0].Length == 0)
                {
                    MessageBox.Show("No password recovery options have been set up for this account.", "No Options Available", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                var resetWindow = new PasswordReset(textAccountName.Text);
                resetWindow.Owner = this;
                if (methods[0].Contains("email"))
                {
                    resetWindow.buttonEmail.Visibility = Visibility.Visible;
                }
                if (methods[0].Contains("question"))
                {
                    resetWindow.buttonQuestion.Visibility = Visibility.Visible;
                    resetWindow.textQuestion.Text = methods[1];
                }
                if (methods[0].Contains("guid"))
                {
                    resetWindow.buttonGUID.Visibility = Visibility.Visible;
                }
                resetWindow.ShowDialog();
            }
        }
        private void checkRememberAccount_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Current.RememberAccount = false;
            Settings.Current.Save();
        }

        private void checkRememberAccount_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Current.RememberAccount = true;
            Settings.Current.Save();
        }
        private void passNewPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            buttonSubmitNewPassword.IsDefault = true;
        }

        private void passNewPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            buttonSubmitNewPassword.IsDefault = false;
        }

        private async void buttonSubmitNewPassword_Click(object sender, RoutedEventArgs e)
        {
            if (passNewPassword.Password.Length == 0)
            {
                MessageBox.Show("Your password can't be empty.", "Password Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (passNewPassword.Password != passConfirmPassword.Password)
            {
                MessageBox.Show("The supplied passwords don't match.", "Passwords Don't Match", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            var result = await Services.UpdatePassword(passNewPassword.Password);
            if (result == null)
            {
                return;
            }
            var strResult = await result.Content.ReadAsStringAsync();
            if (strResult == "false")
            {
                MessageBox.Show("Password update failed.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Password updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                stackNewPassword.Visibility = Visibility.Collapsed;
                gridNewAccount.Visibility = Visibility.Collapsed;
                stackLogin.Visibility = Visibility.Visible;
                gridLogIn.Visibility = Visibility.Visible;
                await LogIn(textAccountName.Text.Trim(), passNewPassword.Password);
            }
        }

        #endregion Login
        #region New Account
        private async void buttonCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            try
            {
                var client = new HttpClient();
                await client.GetAsync(Services.EchoPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The server cannot be reached.  Please check your internet connection." + Environment.NewLine + Environment.NewLine + "Error Details: " + ex.Message, "Network Unavailable", MessageBoxButton.OK, MessageBoxImage.Error);
                buttonCreateAccount.IsEnabled = true;
                return;
            }
            if (textNewAccountName.Text.Length == 0)
            {
                Utilities.ShowStatus("You must enter a username.", Colors.Red);
                buttonCreateAccount.IsEnabled = true;
                return;
            }
            if (passwordNew.Password.Length == 0)
            {
                Utilities.ShowStatus("You must enter a password.", Colors.Red);
                buttonCreateAccount.IsEnabled = true;
                return;
            }
            bool illegalCharacter = false;
            foreach (var character in System.IO.Path.GetInvalidFileNameChars())
            {
                if (textNewAccountName.Text.Contains(character))
                {
                    illegalCharacter = true;
                }
            }

            if (illegalCharacter)
            {
                Utilities.ShowStatus("Username contains invalid characters.", Colors.Red);
                (sender as Button).IsEnabled = true;
                return;
            }

            if (passwordNew.Password != passwordNewConfirm.Password)
            {
                Utilities.ShowStatus("Passwords do not match.", Colors.Red);
                (sender as Button).IsEnabled = true;
                return;
            }
            if (checkLicense.IsChecked != true)
            {
                Utilities.ShowStatus("You must agree to the license terms.", Colors.Red);
                (sender as Button).IsEnabled = true;
                return;
            }
            try
            {
                var response = await Services.CreateAccount(textNewAccountName.Text.Trim(), passwordNew.Password);
                if (response == null)
                {
                    return;
                }
                var strResponse = await response.Content.ReadAsStringAsync();
                if (Guid.TryParse(strResponse, out Guid authCode))
                {
                    Settings.Current.AuthenticationToken = strResponse;
                    Utilities.ShowStatus("New account created!", Colors.Green);
                    await LogIn(textNewAccountName.Text.Trim(), passwordNew.Password);
                }
                else
                {
                    Utilities.ShowStatus("Account name already exists.", Colors.Red);
                }
            }
            catch (Exception ex)
            {
                WriteToLog(ex);
                Utilities.ShowStatus("An error occurred.  Please send a bug report.", Colors.Red);
                (sender as Button).IsEnabled = true;
                return;
            }
            finally
            {
                (sender as Button).IsEnabled = true;
                dataSyncedGames.Items.Refresh();
                Settings.Current.Save();
            }
        }

        private void textCreateAccount_GotFocus(object sender, RoutedEventArgs e)
        {
            buttonCreateAccount.IsDefault = true;
        }

        private void textCreateAccount_LostFocus(object sender, RoutedEventArgs e)
        {
            buttonCreateAccount.IsDefault = false;
        }

        private void hyperLicense_Click(object sender, RoutedEventArgs e)
        {
            var win = new LicenseInfo();
            win.Owner = this;
            win.ShowDialog();
        }
        private void buttonCancelCreate_Click(object sender, RoutedEventArgs e)
        {
            AccountInfo.Current.AccountName = "";
            gridNewAccount.Visibility = Visibility.Collapsed;
            gridLogIn.Visibility = Visibility.Visible;
        }
        #endregion New Account

        #region Tab Control
        private async void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.Source is TabControl) || e.RemovedItems.Count == 0)
            {
                return;
            }
            if (tabMain.SelectedIndex == 0)
            {
                await AnalyzeChanges();
            }
            else if (tabMain.SelectedIndex == 1)
            {
                if (Temp.Current.KnownGames.Count == 0)
                {
                    Utilities.ShowStatus("Retrieving known games...", Colors.Green);
                    var response = await Services.GetKnownGames();
                    if (response == null)
                    {
                        return;
                    }
                    var strResponse = await response.Content.ReadAsStringAsync();
                    if (strResponse.Length > 0)
                    {
                        var knownGames = Json.Decode<List<KnownGame>>(strResponse);
                        if (knownGames != null)
                        {
                            knownGames.Sort((KnownGame a, KnownGame b) => a.Name.CompareTo(b.Name));
                            Temp.Current.KnownGames.Clear();
                            Temp.Current.KnownGames.AddRange(knownGames);
                            comboKnownGames.Items.Refresh();
                        }
                    }
                    Utilities.ShowStatus("Done.", Colors.Green);
                }
            }
            else if (tabMain.SelectedIndex == 4)
            {
                borderNewChatMessage.Visibility = Visibility.Collapsed;
                textNewChatMessage.Text = "0";
                if (Socket == null || (Socket.State != WebSocketState.Open && Socket.State != WebSocketState.Connecting))
                {
                    Socket = SystemClientWebSocket.CreateClientWebSocket();
                    await Socket.ConnectAsync(new Uri("wss://lucency.co/Services/GameCloudChat"), CancellationToken.None);
                    Socket_Handler.HandleSocket(Socket);
                    while (Socket.State != WebSocketState.Open)
                    {
                        await Task.Delay(100);
                    }
                    await Socket_Handler.SocketSend(new {
                        Type = "GetHistory",
                        User = AccountInfo.Current.AccountName
                    });
                }
            }
        }
        #endregion Tab Control

        #region Tab - Synced Games
        private void menuOpenFolder_Click(Object sender, RoutedEventArgs e)
        {
            if (dataSyncedGames.SelectedItems.Count > 0)
            {
                var folderPath = Utilities.ResolveEnvironmentVariables((dataSyncedGames.SelectedItems[0] as SyncedGame).Path);
                Process.Start("explorer.exe", folderPath);
            }
        }

        private async void menuForceUpdate_Click(Object sender, RoutedEventArgs e)
        {
            if (dataSyncedGames.SelectedItems.Count > 0)
            {
                var result = MessageBox.Show("Are you sure you want to force an update for the selected games?  This will replace all files stored on the server with the ones currently on this computer.", "Confirm Update", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var listGames = dataSyncedGames.SelectedItems.Cast<SyncedGame>();
                    foreach (var selectedGame in listGames.ToList())
                    {
                        await Task.Delay(500);
                        var response = await Services.RemoveGame(selectedGame);
                        if (response == null)
                        {
                            return;
                        }
                        if (response.IsSuccessStatusCode)
                        {
                            AccountInfo.Current.Games.Remove(selectedGame);
                            await AnalyzeChanges();
                            dataSyncedGames.Items.Refresh();
                            Settings.Current.Save();
                            var added = Utilities.RoundDateTime(DateTime.Now);
                            var ss = new SyncedGame() {
                                Name = selectedGame.Name.Trim(),
                                Path = selectedGame.Path.Trim(),
                                Platform = selectedGame.Platform,
                                FileFilterOperator = selectedGame.FileFilterOperator,
                                FileFilterPattern = selectedGame.FileFilterPattern,
                                LastLocalSync = added,
                                LastServerSync = added
                            };
                            await AddGame(ss);
                        }
                        else
                        {
                            Utilities.ShowStatus("There was a problem removing the game.  Please try again.", Colors.Red);
                            return;
                        }
                    }
                    Utilities.ShowStatus("Forced update completed.", Colors.Green);
                }
            }
        }
        private async void buttonSync_Click(object sender, RoutedEventArgs e)
        {
            if (dataSyncedGames.SelectedIndex == -1)
            {
                return;
            }
            if (dataSyncedGames.SelectedItem is SyncedGame == false)
            {
                return;
            }
            if (AccountInfo.Current.StorageUsed >= AccountInfo.Current.StorageTotal)
            {
                MessageBox.Show("You've exceeded your storage limit.  The sync feature has been locked.  You must remove games to bring your storage use below the limit.  You can download save files before removing them to retain a local copy.", "Storage Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            await SyncGame();
        }
        private async void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dataSyncedGames.SelectedIndex == -1)
            {
                return;
            }
            if (dataSyncedGames.SelectedItem is SyncedGame == false)
            {
                return;
            }
            var listGames = dataSyncedGames.SelectedItems.Cast<SyncedGame>();
            MessageBoxResult ask;
            if (dataSyncedGames.SelectedItems.Count == 1)
            {
                ask = MessageBox.Show("Are you sure you want to remove " + (dataSyncedGames.SelectedItem as SyncedGame).Name + "?  All save data will be deleted from the Game Cloud server.", "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }
            else
            {
                ask = MessageBox.Show("Are you sure you want to remove " + dataSyncedGames.SelectedItems.Count.ToString() + " games?  All save data will be deleted from the Game Cloud server.", "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }
            if (ask == MessageBoxResult.Yes)
            {
                foreach (var selectedGame in listGames.ToList())
                {
                    await Task.Delay(500);
                    var response = await Services.RemoveGame(selectedGame);
                    if (response == null)
                    {
                        return;
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        AccountInfo.Current.Games.Remove(selectedGame);
                        await AnalyzeChanges();
                        dataSyncedGames.Items.Refresh();
                        Settings.Current.Save();
                        Utilities.ShowStatus("Game removed.", Colors.Green);
                    }
                    else
                    {
                        Utilities.ShowStatus("There was a problem removing the game.  Please try again.", Colors.Red);
                        return;
                    }
                }
            }
            
        }
        private void buttonExport_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.AddExtension = true;
            saveDialog.DefaultExt = ".json";
            saveDialog.Filter = "JSON (*.json)|*.json";
            saveDialog.Title = "Save Game List";
            saveDialog.ShowDialog();
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(saveDialog.FileName)))
                {
                    Temp.Current.BypassAnalysis = true;
                    File.WriteAllText(saveDialog.FileName, Json.Encode(AccountInfo.Current.Games));
                    Utilities.ShowStatus("Game list exported!", Colors.Green);
                    Temp.Current.BypassAnalysis = false;
                }
            }
            catch
            {
                Temp.Current.BypassAnalysis = false;
                Utilities.ShowStatus("Unable to write to the selected path.", Colors.Red);
            }
        }

        private async void buttonImport_Click(object sender, RoutedEventArgs e)
        {
            var openFile = new OpenFileDialog();
            openFile.AddExtension = true;
            openFile.DefaultExt = ".json";
            openFile.Filter = "JSON (*.json)|*.json";
            openFile.Title = "Open Game List";
            openFile.ShowDialog();
            try
            {
                if (File.Exists(openFile.FileName))
                {
                    Temp.Current.BypassAnalysis = true;
                    var clearFirst = MessageBox.Show("Do you want to clear the existing games from your sync list?  If not, they will be merged with the imported list.", "Clear Existing?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    var newList = Json.Decode<List<SyncedGame>>(File.ReadAllText(openFile.FileName));
                    if (clearFirst == MessageBoxResult.Yes)
                    {
                        AccountInfo.Current.Games.Clear();
                    }
                    var currentList = AccountInfo.Current.Games.ToList();
                    newList.AddRange(currentList);
                    
                    AccountInfo.Current.Games.Clear();
                    AccountInfo.Current.Games.AddRange(newList.Distinct());
                    var request = await Services.ImportGames(AccountInfo.Current.Games);
                    if (request == null)
                    {
                        return;
                    }
                    dataSyncedGames.Items.Refresh();
                    Settings.Current.Save();
                    if (request.IsSuccessStatusCode)
                    {
                        Utilities.ShowStatus("Game list imported!", Colors.Green);
                    }
                    else
                    {
                        Utilities.ShowStatus("There was an error importing the list.  Please try again.", Colors.Red);
                    }
                    Temp.Current.BypassAnalysis = false;
                    
                }
            }
            catch
            {
                Utilities.ShowStatus("Unable to import the file.", Colors.Red);
                Temp.Current.BypassAnalysis = false;
            }
        }
        #endregion Tab - Synced Games

        #region Tab - Game Database
        private async void buttonAddGame_Click(object sender, RoutedEventArgs e)
        {
            if (comboKnownGames.SelectedIndex == -1)
            {
                return;
            }
            var selectedGame = comboKnownGames.SelectedItem as KnownGame;
            if(AccountInfo.Current.Games.Exists(sg=>sg.Name == selectedGame.Name))
            {
                Utilities.ShowStatus("The selected game is already synced.", Colors.Red);
                return;
            }
            if (selectedGame.Path.Contains("%steamapps%") && Utilities.SteamAppsFolder == null)
            {
                MessageBox.Show("Steam could not be found on this computer, so this game cannot be synced.", "Steam Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            tabMain.IsEnabled = false;
            Utilities.ShowStatus("Adding save files...", Colors.Green);
            var added = Utilities.RoundDateTime(DateTime.Now);
            var ss = new SyncedGame() {
                Name = selectedGame.Name,
                Path = selectedGame.Path,
                Platform = selectedGame.Platform,
                FileFilterOperator = selectedGame.FileFilterOperator,
                FileFilterPattern = selectedGame.FileFilterPattern,
                LastLocalSync = added,
                LastServerSync = added
            };
            await AddGame(ss);
        }
        private void comboKnownGames_GotFocus(object sender, RoutedEventArgs e)
        {
            if (comboKnownGames.Text == "Filter...")
            {
                comboKnownGames.Text = "";
                comboKnownGames.Foreground = new SolidColorBrush(Colors.Black);
            }
        }
        private void checkFilterInstalledGames_Checked(object sender, RoutedEventArgs e)
        {
            comboKnownGames.Items.Filter = new Predicate<object>((object kg) => Directory.Exists(Utilities.ResolveEnvironmentVariables((kg as KnownGame).Path)) && Directory.GetFiles(Utilities.ResolveEnvironmentVariables((kg as KnownGame).Path), "*", SearchOption.AllDirectories).Length > 0);
        }
        private void checkFilterInstalledGames_Unchecked(object sender, RoutedEventArgs e)
        {
            comboKnownGames.Items.Filter = null;
        }
        private void comboKnownGames_LostFocus(object sender, RoutedEventArgs e)
        {
            if (comboKnownGames.Text == "")
            {
                comboKnownGames.Text = "Filter...";
                comboKnownGames.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }
        private void comboKnownGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboKnownGames.SelectedItem == null)
            {
                return;
            }
            if ((comboKnownGames.SelectedItem as KnownGame).PositiveRatings.Contains(AccountInfo.Current.AccountName))
            {
                toggleRateUp.IsChecked = true;
            }
            else
            {
                toggleRateUp.IsChecked = false;
            }
            if ((comboKnownGames.SelectedItem as KnownGame).NegativeRatings.Contains(AccountInfo.Current.AccountName))
            {
                toggleRateDown.IsChecked = true;
            }
            else
            {
                toggleRateDown.IsChecked = false;
            }
            textTotalVotes.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
            textOverallRating.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
        }
        private void hyperGoToCustom_Click(object sender, RoutedEventArgs e)
        {
            tabMain.SelectedIndex = 2;
        }
        private async void toggleRateUp_Click(object sender, RoutedEventArgs e)
        {
            if (comboKnownGames.SelectedIndex == -1)
            {
                return;
            }
            var kg = (comboKnownGames.SelectedItem as KnownGame);
            kg.NegativeRatings.RemoveAll(name => AccountInfo.Current.AccountName == name);
            kg.PositiveRatings.RemoveAll(name => AccountInfo.Current.AccountName == name);
            if (toggleRateUp.IsChecked == true)
            {
                kg.PositiveRatings.Add(AccountInfo.Current.AccountName);
                toggleRateDown.IsChecked = false;
            }
            textTotalVotes.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
            textOverallRating.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
            await Services.RateGame(kg);
        }
        private async void toggleRateDown_Click(object sender, RoutedEventArgs e)
        {
            if (comboKnownGames.SelectedIndex == -1)
            {
                return;
            }
            var kg = (comboKnownGames.SelectedItem as KnownGame);
            kg.NegativeRatings.RemoveAll(name => AccountInfo.Current.AccountName == name);
            kg.PositiveRatings.RemoveAll(name => AccountInfo.Current.AccountName == name);
            if (toggleRateDown.IsChecked == true)
            {
                if (kg.OverallRating == 0)
                {
                    var diagResult = MessageBox.Show("Your vote will put the game at -1 rating, which will remove it from the database.  Do you wish to proceed?", "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (diagResult == MessageBoxResult.Yes)
                    {
                        kg.NegativeRatings.Add(AccountInfo.Current.AccountName);
                        toggleRateUp.IsChecked = false;
                    }
                    else
                    {
                        toggleRateDown.IsChecked = false;
                    }
                }
            }
            if (kg.OverallRating < 0)
            {
                Temp.Current.KnownGames.Remove(kg);
                comboKnownGames.Items.Refresh();
            }
            else
            {
                textTotalVotes.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
                textOverallRating.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
            }
            await Services.RateGame(kg);
        }
        #endregion Tab - Game Database

        #region Tab - New Game
        private async void buttonAddCustom_Click(object sender, RoutedEventArgs e)
        {
            if (textCustomName.Text.Trim() == "")
            {
                Utilities.ShowStatus("A game name is required.", Colors.Red);
                return;
            }
            if (comboPlatform.Text == "")
            {
                Utilities.ShowStatus("A platform is required.", Colors.Red);
                return;
            }
            if (!Directory.Exists(textCustomPath.Text))
            {
                Utilities.ShowStatus("The path doesn't exist.", Colors.Red);
                return;
            }
           
            if (AccountInfo.Current.Games.Exists(sg => sg.ID == textCustomName.Text + comboPlatform.SelectedItem.ToString() + textCustomPath.Text))
            {
                Utilities.ShowStatus("The selected game is already synced.", Colors.Red);
                return;
            }
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                if (textCustomName.Text.Contains(invalidChar))
                {
                    MessageBox.Show("The game name contains an invalid character.  Please remove any of the following characters: \\/:*?\"<>|", "Invalid Character", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (textFilter.Text.Contains(invalidChar))
                {
                    MessageBox.Show("The file filter contains an invalid character.  Please remove any of the following characters: \\/:*?\"<>|", "Invalid Character", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            tabMain.IsEnabled = false;
            Utilities.ShowStatus("Adding save files...", Colors.Green);
            var added = Utilities.RoundDateTime(DateTime.Now);
            var kg = new KnownGame()
            {
                Name = textCustomName.Text.Trim(),
                Path = Utilities.FormatPathWithVariables(textCustomPath.Text.Trim()),
                Platform = comboPlatform.Text,
                FileFilterOperator = comboFilter.Text,
                FileFilterPattern = textFilter.Text
            };
            if (checkSubmitToDatabase.IsChecked == true && !Temp.Current.KnownGames.Exists(knowngame=> knowngame.ID == kg.ID))
            {
                var response = await Services.AddKnownGame(kg);
                if (response == null)
                {
                    return;
                }
                var strResponse = await response.Content.ReadAsStringAsync();
                Temp.Current.KnownGames.Clear();
                Temp.Current.KnownGames.AddRange(Json.Decode<List<KnownGame>>(strResponse));
                comboKnownGames.Items.Refresh();
            }
            var ss = new SyncedGame()
            {
                Name = textCustomName.Text.Trim(),
                Path = Utilities.FormatPathWithVariables(textCustomPath.Text.Trim()),
                Platform = comboPlatform.Text.Trim(),
                FileFilterOperator = comboFilter.Text,
                FileFilterPattern = textFilter.Text.Trim(),
                LastLocalSync = added,
                LastServerSync = added
            };
            await AddGame(ss);
        }
        private void buttonBrowseCustom_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.ShowDialog();
            if (folderBrowser.SelectedPath.Length > 0)
            {
                textCustomPath.Text = folderBrowser.SelectedPath;
                textCustomPath.Foreground = new SolidColorBrush(Colors.Black);
                TestRootPath();
            }
        }
        private void textCustomPath_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textCustomPath.Text.Contains("Type or paste the path here, or click Browse."))
            {
                textCustomPath.Foreground = new SolidColorBrush(Colors.Black);
                textCustomPath.Text = "";
            }
        }
        private void textCustomPath_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textCustomPath.Text.Trim() == "")
            {
                textCustomPath.Foreground = new SolidColorBrush(Colors.Gray);
                textCustomPath.Text = "Type or paste the path here, or click Browse.";
            }
            else
            {
                textCustomPath.Foreground = new SolidColorBrush(Colors.Black);
                TestRootPath();
            }
        }
        #endregion Tab - New Game
        #region Tab - Ask for Help
        private void radioViewQuestions_Checked(object sender, RoutedEventArgs e)
        {
            borderViewQuestions.Visibility = Visibility.Visible;
            borderAskQuestion.Visibility = Visibility.Collapsed;
        }
        private void radioAskQuestion_Checked(object sender, RoutedEventArgs e)
        {
            if (borderAskQuestion == null || borderViewQuestions == null)
            {
                return;
            }
            borderAskQuestion.Visibility = Visibility.Visible;
            borderViewQuestions.Visibility = Visibility.Collapsed;
        }
        private async void buttonAnswerQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (dataQuestions.SelectedItem == null)
            {
                return;
            }
            var win = new AnswerQuestion();
            win.TheQuestion = dataQuestions.SelectedItem as Question;
            win.Owner = this;
            win.ShowDialog();
            await UpdateQuestions();
        }
        private async void buttonRefreshQuestions_Click(object sender, RoutedEventArgs e)
        {
            await UpdateQuestions();
        }
        private async void buttonSendQuestion_Click(object sender, RoutedEventArgs e)
        {
            var question = new Question()
            {
                AskedOn = DateTime.Now,
                Asker = AccountInfo.Current.AccountName,
                IsAnswered = "No",
                Subject = textAskSubject.Text,
                Message = textAskMessage.Text,
                EmailNotify = checkAskEmailNotification.IsChecked ?? false
            };
            var response = await Services.UpdateHelpRequest(question);
            var strResponse = await response.Content.ReadAsStringAsync();
            if (strResponse == "true")
            {
                MessageBox.Show("Question posted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                textAskSubject.Text = "";
                textAskMessage.Text = "";
            }
            else
            {
                MessageBox.Show("There was an error posting the question.  Please try again or send a bug report.", "Posting Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            await UpdateQuestions();
        }
        private void checkAskEmailNotification_Checked(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(AccountInfo.Current.Email))
            {
                MessageBox.Show("You must have an email saved in your account settings first.", "Email Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                checkAskEmailNotification.IsChecked = false;
                return;
            }
        }
        private void hyperTutorial_Click(object sender, RoutedEventArgs e)
        {
            var welcome = new Welcome();
            welcome.Owner = this;
            welcome.ShowDialog();
        }
        #endregion Tab - Ask for Help

        #region Tab - Chat
        private void textChatInput_GotFocus(object sender, RoutedEventArgs e)
        {
            buttonChatSend.IsDefault = true;
        }

        private void textChatInput_LostFocus(object sender, RoutedEventArgs e)
        {
            buttonChatSend.IsDefault = false;
        }
        private async void buttonChatSend_Click(object sender, RoutedEventArgs e)
        {
            await Socket_Handler.SocketSend(new
            {
                Type = "ChatMessage",
                Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(textChatInput.Text)),
                User = AccountInfo.Current.AccountName,
                Timestamp = DateTime.Now
            });
            textChatInput.Text = "";
        }
        private async void buttonChatUpload_Click(object sender, RoutedEventArgs e)
        {
            var diag = new OpenFileDialog();
            diag.CheckFileExists = true;
            diag.CheckPathExists = true;
            diag.Title = "Upload a file.";
            diag.Multiselect = false;
            diag.ShowDialog();
            if (File.Exists(diag.FileName))
            {
                var runUpload = new TextBlock()
                {
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.DarkCyan),
                    Text = "Upload started.  It will appear here when it's finished."
                };
                textChatWindow.Inlines.Add(runUpload);
                textChatWindow.Inlines.Add(new LineBreak());
                var client = new WebClient();
                var response = await client.UploadFileTaskAsync("https://lucency.co/Services/Downloader/", diag.FileName);
                var strResponse = Encoding.UTF8.GetString(response);
                await Socket_Handler.SocketSend(new
                {
                    Type = "FileShare",
                    URL = strResponse,
                    User = AccountInfo.Current.AccountName,
                    Timestamp = DateTime.Now
                });
            }
        }
        #endregion Tab - Chat

        #region Tab - Feedback
        private void buttonEmailMe_Click(object sender, RoutedEventArgs e)
        {
            var winContact = new Windows.ContactMe();
            winContact.Owner = this;
            winContact.ShowDialog();
        }
        private void buttonReportBug_Click(object sender, RoutedEventArgs e)
        {
            var winBug = new Windows.BugReport();
            winBug.Owner = this;
            winBug.ShowDialog();
        }
        public string AccountName = AccountInfo.Current.AccountName;

        #endregion Tab - Feedback
        #region Helper Methods
        public async Task SyncGame()
        {
            tabMain.IsEnabled = false;
            var totalGames = dataSyncedGames.SelectedItems.Count;
            for (var i = 0; i < totalGames; i++)
            {
                if (i > 0)
                {
                    await Task.Delay(500);
                }
                SyncedGame selectedGame = (SyncedGame)dataSyncedGames.SelectedItems[i];
                var gameTempBackup = Json.Encode(selectedGame);
                await AnalyzeChanges();
                try
                {
                    Directory.CreateDirectory(Utilities.ResolveEnvironmentVariables(selectedGame.Path));
                }
                catch
                {
                    selectedGame.Status = Status.SyncError;
                    continue;
                }
                if (!Directory.Exists(Utilities.ResolveEnvironmentVariables(selectedGame.Path)))
                {
                    continue;
                }

                var gameSaveDir = Utilities.ResolveEnvironmentVariables(selectedGame.Path);
                if (gameSaveDir.Last() != '\\')
                {
                    gameSaveDir += "\\";
                }
                if (!Directory.Exists(gameSaveDir))
                {
                    Directory.CreateDirectory(gameSaveDir);
                }
                if (Settings.Current.CreateLocalBackups)
                {
                    Utilities.ShowStatus("Backing up local files [" + (i + 1).ToString() + " of " + totalGames + "]...", Colors.Green);
                    Directory.CreateDirectory(Utilities.AppDataFolder + @"Backups\");
                    var archive = ZipFile.Open(Utilities.AppDataFolder + @"Backups\" + selectedGame.Name + " " + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + " Local.zip", ZipArchiveMode.Create);
                    foreach (var file in Directory.GetFiles(gameSaveDir, "*", SearchOption.AllDirectories))
                    {
                        if (selectedGame.FileFilterOperator == "Only include")
                        {
                            if (!file.Contains(selectedGame.FileFilterPattern))
                            {
                                continue;
                            }
                        }
                        else if (selectedGame.FileFilterOperator == "Exclude all")
                        {
                            if (file.Contains(selectedGame.FileFilterPattern))
                            {
                                continue;
                            }
                        }
                        archive.CreateEntryFromFile(file, file.Replace(gameSaveDir, ""));
                    }
                    archive.Dispose();
                    TrimBackups();
                }

                Utilities.ShowStatus("Comparing game files [" + (i + 1).ToString() + " of " + totalGames + "]...", Colors.Green);
                var remoteGame = Temp.Current.RemoteAccount.Games.Find(sg => sg.Name == selectedGame.Name);
                foreach (var remoteFile in remoteGame.FileList)
                {
                    if (!selectedGame.FileList.Exists(gfi => gfi.FileName == remoteFile.FileName))
                    {
                        selectedGame.FileList.Add(remoteFile);
                    }
                    var localFile = selectedGame.FileList.Find(gfi => gfi.FileName == remoteFile.FileName);
                    if (!File.Exists(gameSaveDir + remoteFile.RelativePath) || localFile.LastWriteTime == null || remoteFile.LastWriteTime > localFile?.LastWriteTime)
                    {
                        if (localFile == null)
                        {
                            selectedGame.FileList.Add(new GameFileInfo()
                            {
                                FileName = localFile.FileName,
                                RelativePath = remoteFile.RelativePath,
                                LastWriteTime = Utilities.RoundDateTime((DateTime)remoteFile.LastWriteTime)
                            });
                        }
                        else
                        {
                            localFile.LastWriteTime = Utilities.RoundDateTime((DateTime)remoteFile.LastWriteTime);
                        }
                        Directory.CreateDirectory(Path.GetDirectoryName(gameSaveDir + remoteFile.RelativePath));
                        var response = await Services.GetFile(selectedGame, remoteFile.RelativePath);
                        if (response == null)
                        {
                            return;
                        }
                        var byteArr = await response.Content.ReadAsByteArrayAsync();
                        try
                        {
                            File.WriteAllBytes(gameSaveDir + remoteFile.RelativePath, byteArr);
                        }
                        catch
                        {
                            MessageBox.Show("There was a problem downloading file to " + localFile.FileName + ".  Make sure Game Cloud is running as an administrator and that you have write access to the location.", "Download Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }
                    }
                }

                var strLocalFiles = Directory.GetFiles(gameSaveDir, "*", SearchOption.AllDirectories).ToList();
                
                foreach (var strLocalFile in strLocalFiles)
                {
                    var localFile = new FileInfo(strLocalFile);
                    var remoteFile = remoteGame.FileList.Find(gfi => gfi.FileName == localFile.Name);

                    if (selectedGame.FileFilterOperator == "Only include")
                    {
                        if (!localFile.Name.ToLower().Contains(selectedGame.FileFilterPattern.ToLower()))
                        {
                            continue;
                        }
                    }
                    else if (selectedGame.FileFilterOperator == "Exclude all")
                    {
                        if (localFile.Name.ToLower().Contains(selectedGame.FileFilterPattern.ToLower()))
                        {
                            continue;
                        }
                    }

                    if (!selectedGame.FileList.Exists(gfi => gfi.FileName == localFile.Name))
                    {
                        selectedGame.FileList.Add(new GameFileInfo()
                        {
                            FileName = localFile.Name,
                            RelativePath = strLocalFile.Replace(gameSaveDir, "\\"),
                            LastWriteTime = Utilities.RoundDateTime(localFile.LastWriteTimeUtc)
                        });
                    }
                    else
                    {
                        selectedGame.FileList.Find(gfi => gfi.FileName == localFile.Name).LastWriteTime = Utilities.RoundDateTime(localFile.LastWriteTimeUtc);
                    }
                    if (remoteFile == null || remoteFile.LastWriteTime == null || localFile.LastWriteTimeUtc > remoteFile?.LastWriteTime)
                    {
                        var response = await Services.UploadFile(selectedGame, strLocalFile, strLocalFile.Replace(gameSaveDir, "\\"));
                        if (response == null)
                        {
                            return;
                        }
                        if (response == false)
                        {
                            selectedGame = Json.Decode<SyncedGame>(gameTempBackup);
                            MessageBox.Show("There was a problem uploading file " + localFile.Name + ".", "Upload Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            tabMain.IsEnabled = true;
                            continue;
                        }
                    }
                }

                var updated = Utilities.RoundDateTime(DateTime.Now);
                selectedGame.LastLocalSync = updated;
                selectedGame.LastServerSync = updated;
                selectedGame.Status = Status.OK;
                selectedGame.StatusDetails = "This game is up-to-date.";
                long size = 0;
                foreach (var fi in new DirectoryInfo(gameSaveDir).GetFiles("*", SearchOption.AllDirectories))
                {
                    if (selectedGame.FileFilterOperator == "Only include")
                    {
                        if (!fi.Name.ToLower().Contains(selectedGame.FileFilterPattern.ToLower()))
                        {
                            continue;
                        }
                    }
                    else if (selectedGame.FileFilterOperator == "Exclude all")
                    {
                        if (fi.Name.ToLower().Contains(selectedGame.FileFilterPattern.ToLower()))
                        {
                            continue;
                        }
                    }
                    size += fi.Length;
                }
                selectedGame.StorageUse = Math.Ceiling((double)size / 1024 / 1024);
                Utilities.ShowStatus("Syncing changes [" + (i + 1).ToString() + " of " + totalGames + "]...", Colors.Green);
                
                var response2 = await Services.SyncGame(selectedGame);
                if (response2 == null)
                {
                    return;
                }
                if (response2 == true)
                {
                    Utilities.ShowStatus("Game synced successfully.", Colors.Green);
                    dataSyncedGames.Items.Refresh();
                }
                else
                {
                    selectedGame = Json.Decode<SyncedGame>(gameTempBackup);
                    Utilities.ShowStatus("There was a problem syncing the game.", Colors.Red);
                }
            }
            await RetrieveAccount();
            tabMain.IsEnabled = true;
        }
        private async Task AddGame(SyncedGame SyncedGame)
        {
            var ss = SyncedGame;
            var contentPath = Utilities.ResolveEnvironmentVariables(ss.Path);
            if (!Directory.Exists(contentPath))
            {
                var ask = MessageBox.Show("The game's save directory (" + contentPath + ") wasn't found.  Would you like to create it?", "Create Directory?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ask == MessageBoxResult.Yes)
                {
                    Directory.CreateDirectory(contentPath);
                }
                else
                {
                    Utilities.ShowStatus("Directory wasn't found.", Colors.Red);
                    tabMain.IsEnabled = true;
                    return;
                }
            }
            long size = 0;
            foreach (var fileInfo in new DirectoryInfo(contentPath).GetFiles("*", SearchOption.AllDirectories))
            {
                if (ss.FileFilterOperator == "Only include")
                {
                    if (!fileInfo.Name.ToLower().Contains(ss.FileFilterPattern.ToLower()))
                    {
                        continue;
                    }
                }
                else if (ss.FileFilterOperator == "Exclude all")
                {
                    if (fileInfo.Name.ToLower().Contains(ss.FileFilterPattern.ToLower()))
                    {
                        continue;
                    }
                }
                ss.FileList.Add(new GameFileInfo()
                {
                    FileName = fileInfo.Name,
                    LastWriteTime = fileInfo.LastWriteTimeUtc,
                    RelativePath = fileInfo.FullName.Replace(contentPath, "\\")
                });
                size += fileInfo.Length;
                var result = await Services.UploadFile(ss, fileInfo.FullName, fileInfo.FullName.Replace(contentPath, "\\"));
                if (result == null)
                {
                    return;
                }
                if (result == false)
                {
                    Utilities.ShowStatus("Unable to add game.", Colors.Red);
                    tabMain.IsEnabled = true;
                    return;
                }
            }
            ss.StorageUse = Math.Ceiling((double)size / 1024 / 1024);
            var response = await Services.AddGame(ss);
            if (response == null)
            {
                return;
            }
            if (response == true)
            {
                AccountInfo.Current.Games.Add(ss);
                await AnalyzeChanges();
                dataSyncedGames.Items.Refresh();
                tabMain.SelectedIndex = 0;
                Settings.Current.Save();
                Utilities.ShowStatus("Game added and synced.", Colors.Green);
                tabMain.IsEnabled = true;
                return;
            }
            else
            {
                Utilities.ShowStatus("Unable to add game.", Colors.Red);
                tabMain.IsEnabled = true;
                return;
            }
        }
        private async Task LogIn(string Username, string Password)
        {
            try
            {
                AccountInfo.Current.AccountName = Username;
                var responseCheckAccount = await Services.CheckAccount(Password);
                if (responseCheckAccount == null)
                {
                    return;
                }
                if (!responseCheckAccount.IsSuccessStatusCode)
                {
                    MessageBox.Show("The server cannot be reached.  Please check your internet connection.", "Network Unavailable", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var strResponseCheckAccount = await responseCheckAccount.Content.ReadAsStringAsync();
                if (strResponseCheckAccount == "false")
                {
                    Settings.Current.AuthenticationToken = "";
                    passPassword.Password = "";
                    Utilities.ShowStatus("Incorrect username or password.  Try again or create a new account.", Colors.Red);
                    return;
                }
                else if (strResponseCheckAccount == "expired")
                {
                    Settings.Current.AuthenticationToken = "";
                    Utilities.ShowStatus("Authentication token expired.  Please log in again.", Colors.Red);
                    passPassword.Password = "";
                    return;
                }
                else if (strResponseCheckAccount.Contains("newpassword"))
                {
                    Settings.Current.AuthenticationToken = strResponseCheckAccount.Split(',')[1];
                    stackLogin.Visibility = Visibility.Collapsed;
                    stackNewPassword.Visibility = Visibility.Visible;
                }
                else if (Guid.TryParse(strResponseCheckAccount, out Guid authCode))
                {
                    Settings.Current.AuthenticationToken = strResponseCheckAccount;
                    if (checkRememberAccount.IsChecked == true)
                    {
                        Settings.Current.RememberAccount = true;
                        Settings.Current.LastUser = textAccountName.Text;
                    }
                    else
                    {
                        Settings.Current.RememberAccount = false;
                        passPassword.Password = "";
                    }
                    gridLogIn.Visibility = Visibility.Collapsed;
                    gridNewAccount.Visibility = Visibility.Collapsed;
                    this.MinWidth = 600;
                    this.Width = 600;
                    this.MinHeight = 475;
                    this.Height = 475;
                    this.Left -= 125;
                    if (this.Left < 0)
                    {
                        this.Left = 0;
                    }
                    gridGames.Visibility = Visibility.Visible;
                    Settings.Current.LoadUser(AccountInfo.Current.AccountName);
                    Utilities.ShowStatus("Retrieving account...", Colors.Green);
                    await AnalyzeChanges();
                    Utilities.ShowStatus("Done.", Colors.Green);
                    menuAccount.IsEnabled = true;
                    menuOptions.IsEnabled = true;
                    gridTitle.Visibility = Visibility.Collapsed;
                    gridAccountInfo.Visibility = Visibility.Visible;
                    if (Settings.Current.ShowHelpRequests)
                    {
                        await UpdateQuestions();
                    }
                    if (AccountInfo.Current.Games.Count == 0)
                    {
                        var welcome = new Welcome();
                        welcome.Owner = this;
                        welcome.ShowDialog();
                    }
                }
                else
                {
                    Utilities.ShowStatus("Unknown error.", Colors.Red);
                }
            }
            catch (Exception ex)
            {
                WriteToLog(ex);
                Utilities.ShowStatus("An error occurred.  Please send a bug report.", Colors.Red);
            }
            finally
            {
                buttonLogIn.IsEnabled = true;
            }
        }
        public async Task<bool> RetrieveAccount()
        {
            var responseGetAccount = await Services.GetAccount();
            if (responseGetAccount == null)
            {
                return false;
            }
            var strResponseGetAccount = await responseGetAccount.Content.ReadAsStringAsync();
            Temp.Current.RemoteAccount = Json.Decode<AccountInfo>(strResponseGetAccount);
            AccountInfo.Current.StorageUsed = Temp.Current.RemoteAccount.StorageUsed;
            AccountInfo.Current.StorageTotal = Temp.Current.RemoteAccount.StorageTotal;
            AccountInfo.Current.IsEmailEnabled = Temp.Current.RemoteAccount.IsEmailEnabled;
            AccountInfo.Current.IsQuestionEnabled = Temp.Current.RemoteAccount.IsQuestionEnabled;
            AccountInfo.Current.IsMachineGUIDEnabled = Temp.Current.RemoteAccount.IsMachineGUIDEnabled;
            AccountInfo.Current.Email = Temp.Current.RemoteAccount.Email;
            AccountInfo.Current.ChallengeQuestion = Temp.Current.RemoteAccount.ChallengeQuestion;
            AccountInfo.Current.ChallengeResponse = Temp.Current.RemoteAccount.ChallengeResponse;
            Settings.Current.Save();
            return true;
        }
        public async Task AnalyzeChanges()
        {
            if (Temp.Current.BypassAnalysis)
            {
                return;
            }
            Utilities.ShowStatus("Analyzing remote changes...", Colors.Green);
            var result = await RetrieveAccount();
            if (!result)
            {
                return;
            }
            
            foreach (var remoteGame in Temp.Current.RemoteAccount.Games)
            {
                var localGame = AccountInfo.Current.Games.Find(sg => sg.Name == remoteGame.Name);
                if (localGame == null)
                {
                    AccountInfo.Current.Games.Add(new SyncedGame()
                    {
                        Name = remoteGame.Name,
                        Path = remoteGame.Path,
                        Platform = remoteGame.Platform,
                        FileFilterOperator = remoteGame.FileFilterOperator,
                        FileFilterPattern = remoteGame.FileFilterPattern,
                        LastServerSync = remoteGame.LastServerSync,
                        Status = Status.DownloadAvailable,
                        StatusDetails = "Changes are available for download.",
                    });
                }
                else
                {
                    localGame.StorageUse = remoteGame.StorageUse;
                    if (localGame.LastLocalSync != remoteGame.LastServerSync)
                    {
                        localGame.Status = Status.DownloadAvailable;
                        localGame.StatusDetails = "Changes are available for download.";
                    }
                    foreach (var file in remoteGame.FileList)
                    {
                        if (!File.Exists(Utilities.ResolveEnvironmentVariables(remoteGame.Path) + "\\" + file.RelativePath))
                        {
                            localGame.Status = Status.DownloadAvailable;
                            localGame.StatusDetails = "Changes are available for download.";
                        }
                    }
                }
            }

            Utilities.ShowStatus("Analyzing local changes...", Colors.Green);
            var listRemove = new List<SyncedGame>();
            foreach (var game in AccountInfo.Current.Games)
            {
                if (!Temp.Current.RemoteAccount.Games.Exists(sg => sg.Name == game.Name))
                {
                    listRemove.Add(game);
                    continue;
                }
                if (game.Status == Status.Error || game.Status == Status.SyncError || game.Status == Status.FolderPathUnknown || game.Status == Status.SavePathNotFound) 
                {
                    continue;
                }
                
                foreach (var file in Directory.GetFiles(Utilities.ResolveEnvironmentVariables(game.Path), "*", SearchOption.AllDirectories))
                {
                    var fi = new FileInfo(file);
                    if (game.FileFilterOperator == "Only include")
                    {
                        if (!fi.Name.ToLower().Contains(game.FileFilterPattern.ToLower()))
                        {
                            continue;
                        }
                    }
                    else if (game.FileFilterOperator == "Exclude all")
                    {
                        if (fi.Name.ToLower().Contains(game.FileFilterPattern.ToLower()))
                        {
                            continue;
                        }
                    }
                    var lastWriteTime = fi.LastWriteTime;
                    var creationTime = File.GetCreationTime(file);
                    if (lastWriteTime > game.LastLocalSync || creationTime > game.LastLocalSync)
                    {
                        if (!game.Status.Contains(Status.DownloadAvailable))
                        {
                            game.Status = Status.UploadAvailable;
                            game.StatusDetails = "Changes are available for upload.";
                        }
                        else if (!game.Status.Contains(Status.DownloadUploadAvailable))
                        {
                            game.Status = Status.DownloadUploadAvailable;
                            game.StatusDetails = "Changes are available for both download and upload.";
                        }
                    }
                }
                if (game.Status == Status.OK)
                {
                    game.StatusDetails = "This game is up-to-date.";
                }
            }
            foreach (var game in listRemove)
            {
                AccountInfo.Current.Games.Remove(game);
            }
            Settings.Current.Save();
            dataSyncedGames.Items.Refresh();
            Utilities.ShowStatus("", Colors.Green);
        }
        public async void LogOut()
        {
            tabMain.SelectedIndex = 0;
            gridAccountInfo.Visibility = Visibility.Collapsed;
            gridGames.Visibility = Visibility.Collapsed;
            gridLogIn.Visibility = Visibility.Visible;
            gridNewAccount.Visibility = Visibility.Collapsed;
            gridTitle.Visibility = Visibility.Visible;
            Settings.Current.LastUser = "";
            AccountInfo.Current.Games.Clear();
            dataSyncedGames.Items.Refresh();
            menuAccount.IsEnabled = false;
            menuOptions.IsEnabled = false;
            if (Settings.Current.RememberAccount == false)
            {
                passPassword.Password = "";
            }
            Utilities.ShowStatus("Logged out.", Colors.Green);
            this.MinWidth = 350;
            this.MinHeight = 400;
            this.Width = 350;
            this.Height = 400;
            if (Socket != null && Socket.State == WebSocketState.Open)
            {
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Logged out.", CancellationToken.None);
            }
        }
        private async Task CheckForUpdates(bool Silent)
        {
            WebClient webClient = new WebClient();
            var strFilePath = Path.GetTempPath() + "Game Cloud.exe";
            HttpResponseMessage response;
            if (File.Exists(strFilePath))
            {
                File.Delete(strFilePath);
            }
            try
            {
                response = await Services.GetCurrentVersion();   
            }
            catch
            {
                if (!Silent)
                {
                    MessageBox.Show("Unable to contact the server.  Check your network connection or try again later.", "Server Unreachable", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                return;
            }
            var strCurrentVersion = await response.Content.ReadAsStringAsync();
            var thisVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var currentVersion = Version.Parse(strCurrentVersion);
            if (currentVersion > thisVersion)
            {
                var result = MessageBox.Show("A new version of Game Cloud is available!  Would you like to download it now?  It's an instant and effortless process.", "New Version Available", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    await webClient.DownloadFileTaskAsync(new Uri("https://lucency.co/Downloads/Game Cloud.exe"), strFilePath);
                    Process.Start(strFilePath, "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
                    App.Current.Shutdown();
                    return;
                }
            }
            else
            {
                if (!Silent)
                {
                    MessageBox.Show("Game Cloud is up-to-date.", "No Updates", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        public void TrimBackups()
        {
            double totalSize = 0;
            List<FileInfo> listFiles = new List<FileInfo>();
            var strFiles = Directory.GetFiles(Utilities.AppDataFolder + @"Backups\", "*", SearchOption.AllDirectories);
            foreach (var file in strFiles)
            {
                var fi = new FileInfo(file);
                listFiles.Add(fi);
                totalSize += fi.Length;
            }
            listFiles.Sort((FileInfo a, FileInfo b) => a.LastWriteTimeUtc.CompareTo(b.LastWriteTimeUtc));
            while (totalSize / 1024 / 1024 > Settings.Current.MaximumBackupSize || listFiles.Count > Settings.Current.MaximumBackupCount)
            {
                totalSize -= listFiles[0].Length;
                listFiles.RemoveAt(0);
            }
        }
        private void TestRootPath()
        {
            if (textCustomPath.Text.Length == 0)
            {
                return;
            }
            for (int i = 0; i < textCustomPath.Text.Length; i++)
            {
                while (i <= textCustomPath.Text.Length - 1 && Path.GetInvalidPathChars().Any(ch => ch == textCustomPath.Text[i]))
                {
                    textCustomPath.Text = textCustomPath.Text.Remove(i, 1);
                }
            }
            if (Path.GetPathRoot(textCustomPath.Text) != Path.GetPathRoot(Environment.SystemDirectory))
            {
                checkSubmitToDatabase.IsChecked = false;
                checkSubmitToDatabase.IsEnabled = false;
                textPathNotSystemRoot.Visibility = Visibility.Visible;
            }
            else
            {
                checkSubmitToDatabase.IsEnabled = true;
                textPathNotSystemRoot.Visibility = Visibility.Collapsed;
            }
        }
        public async Task UpdateQuestions()
        {
            Utilities.ShowStatus("Retrieving help requests...", Colors.Green);
            var response = await Services.GetHelpRequests();
            if (response == null)
            {
                return;
            }
            var strResponse = await response.Content.ReadAsStringAsync();
            if (!String.IsNullOrWhiteSpace(strResponse))
            {
                var questions = Json.Decode<List<Question>>(strResponse);
                Temp.Current.Questions.Clear();
                foreach (var question in questions)
                {
                    Temp.Current.Questions.Add(question);
                }
                dataQuestions.Items.Refresh();
                var unanswered = questions.Count(q => q.IsAnswered == "No");
                if (unanswered > 0)
                {
                    borderHelpRequestsAvailable.Visibility = Visibility.Visible;
                    textHelpRequestsAvailable.Text = unanswered.ToString();
                }
                else
                {
                    borderHelpRequestsAvailable.Visibility = Visibility.Collapsed;
                    textHelpRequestsAvailable.Text = "0";
                }
            }
            Utilities.ShowStatus("", Colors.Green);
        }
        public void WriteToLog(Exception ExMessage)
        {
            Directory.CreateDirectory(Utilities.AppDataFolder);
            var ex = ExMessage;
            while (ex != null)
            {
                File.AppendAllText(Utilities.AppDataFolder + @"ErrorLog.txt", DateTime.Now.ToString() + "\t" + ex.Message + "\t" + ex.Source + "\t" + ex.StackTrace + Environment.NewLine);
                ex = ex.InnerException;
            }
        }
        #endregion Helper Methods
    }
}
