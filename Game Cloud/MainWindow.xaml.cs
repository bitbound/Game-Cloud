using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml;
using Game_Cloud.Models;
using System.IO.Compression;
using System.Windows.Media.Animation;
using System.Security.Cryptography;
using System.Diagnostics;
using Microsoft.Win32;
using Game_Cloud.Windows;
using System.Data;

namespace Game_Cloud
{
    public partial class MainWindow : Window
    {
        public static MainWindow Current { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = Settings.Current;
            comboKnownGames.DataContext = SettingsTemp.Current;
            menuAccount.DataContext = SettingsTemp.Current;
            Current = this;

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1]))
            {
                var count = 0;
                var success = false;
                while (success == false)
                {
                    System.Threading.Thread.Sleep(200);
                    count++;
                    if (count > 25)
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
                        WriteToLog(ex.Message + ex.StackTrace);
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
#if !DEBUG
            App.Current.DispatcherUnhandledException += DispatcherUnhandledException;
#endif
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
                passPassword.Password = Settings.Current.LastPassword;
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
            gridAccountInfo.Visibility = Visibility.Collapsed;
            gridGames.Visibility = Visibility.Collapsed;
            gridLogIn.Visibility = Visibility.Visible;
            gridNewAccount.Visibility = Visibility.Collapsed;
            gridTitle.Visibility = Visibility.Visible;
            Settings.Current.Save();
            Settings.Current.LastUser = "";
            Settings.Current.AccountInfo.Games.Clear();
            dataSyncedGames.Items.Refresh();
            menuAccount.IsEnabled = false;
            stackNoGames.Visibility = Visibility.Collapsed;
            if (Settings.Current.RememberAccount == false)
            {
                passPassword.Password = "";
            }
            Utilities.ShowStatus("Logged out.", Colors.Green);
            this.Width = 350;
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

        private void menuFAQ_Click(object sender, RoutedEventArgs e)
        {
            var win = new Windows.FAQ();
            win.Owner = this;
            win.ShowDialog();
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

        private void hyperGetMoreStorage_Click(object sender, RoutedEventArgs e)
        {
            var win = new Windows.Subscribe();
            win.Owner = App.Current.MainWindow;
            win.ShowDialog();
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
            WriteToLog(e.Exception.Message + "\t" + e.Exception.StackTrace);
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
        private async void buttonLogIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new HttpClient();
                await client.GetAsync(Services.ServicePath + "EchoIP");
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
            await LogIn(textAccountName.Text, passPassword.Password);
        }
        private async void hyperForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            if (textAccountName.Text.Length == 0)
            {
                MessageBox.Show("You must enter an account name to recover your password.", "Account Name Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            var response = await Services.RecoverPassword(textAccountName.Text, null, null);
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
       

        #endregion Login
        #region New Account
        private async void buttonCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;
            try
            {
                var client = new HttpClient();
                await client.GetAsync(Services.ServicePath + "EchoIP");
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
                var response = await Services.CreateAccount(textNewAccountName.Text, HashHelper.HashString(passwordNew.Password));
                if (await response.Content.ReadAsStringAsync() == "true")
                {
                    Utilities.ShowStatus("New account created!", Colors.Green);
                    await LogIn(textNewAccountName.Text, passwordNew.Password);
                }
                else
                {
                    Utilities.ShowStatus("Account name already exists.", Colors.Red);
                }
            }
            catch (Exception ex)
            {
                Utilities.ShowStatus("Error: " + ex.Message, Colors.Red);
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
            Settings.Current.AccountInfo.AccountName = "";
            Settings.Current.AccountInfo.Password = "";
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
                if (SettingsTemp.Current.KnownGames.Count == 0)
                {
                    Utilities.ShowStatus("Retrieving known games...", Colors.Green);
                    var response = await Services.GetKnownGames();
                    var strResponse = await response.Content.ReadAsStringAsync();
                    if (strResponse.Length > 0)
                    {
                        var knownGames = JsonHelper.Decode<List<KnownGame>>(strResponse);
                        if (knownGames != null)
                        {
                            knownGames.Sort((KnownGame a, KnownGame b) => a.Name.CompareTo(b.Name));
                            SettingsTemp.Current.KnownGames.Clear();
                            SettingsTemp.Current.KnownGames.AddRange(knownGames);
                            comboKnownGames.Items.Refresh();
                        }
                    }
                    Utilities.ShowStatus("Done.", Colors.Green);
                }
            }
        }
        #endregion Tab Control

        #region Tab - Synced Games
        private void dataSyncedGames_PreviewMouseRightButtonUp(Object sender, MouseButtonEventArgs e)
        {
            dataSyncedGames.ContextMenu.IsOpen = true;
            if (dataSyncedGames.SelectedItems.Count > 0)
            {
                dataSyncedGames.ContextMenu.IsEnabled = true;
            }
            else
            {
                dataSyncedGames.ContextMenu.IsEnabled = false;
            }
        }
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
                        if (response.IsSuccessStatusCode)
                        {
                            Settings.Current.AccountInfo.Games.Remove(selectedGame);
                            await AnalyzeChanges();
                            dataSyncedGames.Items.Refresh();
                            Settings.Current.Save();
                            var added = Utilities.RoundDateTime(DateTime.Now);
                            var ss = new SyncedGame() { Name = selectedGame.Name, Path = selectedGame.Path, LastLocalSync = added, LastServerSync = added };
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
            if (Settings.Current.AccountInfo.StorageUsed >= Settings.Current.AccountInfo.StorageTotal)
            {
                MessageBox.Show("You've exceeded your storage limit.  The sync feature has been locked.  You must remove games to bring your storage use below the limit or increase your storage limit by subscribing.  You can download save files before removing them to retain a local copy.", "Storage Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
                    if (response.IsSuccessStatusCode)
                    {
                        Settings.Current.AccountInfo.Games.Remove(selectedGame);
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
                    SettingsTemp.Current.BypassAnalysis = true;
                    File.WriteAllText(saveDialog.FileName, JsonHelper.Encode(Settings.Current.AccountInfo.Games));
                    Utilities.ShowStatus("Game list exported!", Colors.Green);
                    SettingsTemp.Current.BypassAnalysis = false;
                }
            }
            catch
            {
                SettingsTemp.Current.BypassAnalysis = false;
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
                    SettingsTemp.Current.BypassAnalysis = true;
                    var clearFirst = MessageBox.Show("Do you want to clear the existing games from your sync list?  If not, they will be merged with the imported list.", "Clear Existing?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    var newList = JsonHelper.Decode<List<SyncedGame>>(File.ReadAllText(openFile.FileName));
                    if (clearFirst == MessageBoxResult.Yes)
                    {
                        Settings.Current.AccountInfo.Games.Clear();
                    }
                    var currentList = Settings.Current.AccountInfo.Games.ToList();
                    newList.AddRange(currentList);
                    
                    Settings.Current.AccountInfo.Games.Clear();
                    Settings.Current.AccountInfo.Games.AddRange(newList.Distinct());
                    var request = await Services.ImportGames(Settings.Current.AccountInfo.Games);
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
                    SettingsTemp.Current.BypassAnalysis = false;
                    
                }
            }
            catch
            {
                Utilities.ShowStatus("Unable to import the file.", Colors.Red);
                SettingsTemp.Current.BypassAnalysis = false;
            }
        }
        private void hyperGoToAdd_Click(object sender, RoutedEventArgs e)
        {
            stackNoGames.Visibility = Visibility.Collapsed;
            tabMain.SelectedIndex = 1;
        }
        #endregion Tab - Synced Games

        #region Tab - Add Game
        private async void buttonAddGame_Click(object sender, RoutedEventArgs e)
        {
            if (comboKnownGames.SelectedIndex == -1)
            {
                return;
            }
            var selectedGame = comboKnownGames.SelectedItem as KnownGame;
            if(Settings.Current.AccountInfo.Games.Exists(sg=>sg.Name == selectedGame.Name))
            {
                Utilities.ShowStatus("The selected game is already synced.", Colors.Red);
                return;
            }
            if (selectedGame.Path.Contains("%steamapps%") && SettingsTemp.SteamAppsFolder == null)
            {
                MessageBox.Show("Steam could not be found on this computer, so this game cannot be synced.", "Steam Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            tabMain.IsEnabled = false;
            Utilities.ShowStatus("Adding save files...", Colors.Green);
            var added = Utilities.RoundDateTime(DateTime.Now);
            var ss = new SyncedGame() { Name = selectedGame.Name, Path = selectedGame.Path, LastLocalSync = added, LastServerSync = added };
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
            if ((comboKnownGames.SelectedItem as KnownGame).PositiveRatings.Contains(Settings.Current.AccountInfo.AccountName))
            {
                toggleRateUp.IsChecked = true;
            }
            else
            {
                toggleRateUp.IsChecked = false;
            }
            if ((comboKnownGames.SelectedItem as KnownGame).NegativeRatings.Contains(Settings.Current.AccountInfo.AccountName))
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
            kg.NegativeRatings.RemoveAll(name => Settings.Current.AccountInfo.AccountName == name);
            kg.PositiveRatings.RemoveAll(name => Settings.Current.AccountInfo.AccountName == name);
            if (toggleRateUp.IsChecked == true)
            {
                kg.PositiveRatings.Add(Settings.Current.AccountInfo.AccountName);
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
            kg.NegativeRatings.RemoveAll(name => Settings.Current.AccountInfo.AccountName == name);
            kg.PositiveRatings.RemoveAll(name => Settings.Current.AccountInfo.AccountName == name);
            if (toggleRateDown.IsChecked == true)
            {
                kg.NegativeRatings.Add(Settings.Current.AccountInfo.AccountName);
                toggleRateUp.IsChecked = false;
            }
            if (kg.OverallRating < 0)
            {
                SettingsTemp.Current.KnownGames.Remove(kg);
                comboKnownGames.Items.Refresh();
            }
            else
            {
                textTotalVotes.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
                textOverallRating.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
            }
            await Services.RateGame(kg);
        }

        #endregion Tab - Add Game

        #region Tab - Add Custom
        private async void buttonAddCustom_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(textCustomPath.Text))
            {
                Utilities.ShowStatus("The path doesn't exist.", Colors.Red);
                return;
            }
            if (Settings.Current.AccountInfo.Games.Exists(sg => sg.Name == textCustomName.Text))
            {
                Utilities.ShowStatus("The selected game is already synced.", Colors.Red);
                return;
            }
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                if (textCustomName.Text.Contains(invalidChar))
                {
                    MessageBox.Show("The game name contains an invalid character.", "Invalid Character", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            tabMain.IsEnabled = false;
            Utilities.ShowStatus("Adding save files...", Colors.Green);
            var added = Utilities.RoundDateTime(DateTime.Now);
            var kg = new KnownGame() { Name = textCustomName.Text, Path = Utilities.FormatPathWithVariables(textCustomPath.Text) };
            if (checkSubmitToDatabase.IsChecked == true && !SettingsTemp.Current.KnownGames.Exists(knowngame=> knowngame.Name == kg.Name))
            {
                var response = await Services.AddKnownGame(kg);
                var strResponse = await response.Content.ReadAsStringAsync();
                SettingsTemp.Current.KnownGames.Clear();
                SettingsTemp.Current.KnownGames.AddRange(JsonHelper.Decode<List<KnownGame>>(strResponse));
                comboKnownGames.Items.Refresh();
            }
            var ss = new SyncedGame() { Name = textCustomName.Text, Path = Utilities.FormatPathWithVariables(textCustomPath.Text), LastLocalSync = added, LastServerSync = added };
            await AddGame(ss);
        }
        private void buttonBrowseCustom_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.ShowDialog();
            if (folderBrowser.SelectedPath.Length > 0)
            {
                textCustomPath.Text = folderBrowser.SelectedPath;
            }
        }
        #endregion Tab - Add Custom
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
        public string AccountName = Settings.Current.AccountInfo.AccountName;

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
                var gameTempBackup = JsonHelper.Encode(selectedGame);
                await AnalyzeChanges();
                try
                {
                    Directory.CreateDirectory(Utilities.ResolveEnvironmentVariables(selectedGame.Path));
                }
                catch
                {
                    selectedGame.Status = "⚠";
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
                    Directory.CreateDirectory(SettingsTemp.AppDataFolder + @"Backups\");
                    await Task.Run(() => ZipFile.CreateFromDirectory(gameSaveDir, SettingsTemp.AppDataFolder + @"Backups\" + selectedGame.Name + " " + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + " Local.zip"));
                    TrimBackups();
                }

                Utilities.ShowStatus("Comparing game files [" + (i + 1).ToString() + " of " + totalGames + "]...", Colors.Green);
                var remoteGame = SettingsTemp.Current.RemoteAccount.Games.Find(sg => sg.Name == selectedGame.Name);
                foreach (var remoteFile in remoteGame.FileList)
                {
                    if (!selectedGame.FileList.Exists(gfi => gfi.FileName == remoteFile.FileName))
                    {
                        selectedGame.FileList.Add(remoteFile);
                    }
                    var localFile = selectedGame.FileList.Find(gfi => gfi.FileName == remoteFile.FileName);
                    if (!File.Exists(gameSaveDir + remoteFile.RelativePath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(gameSaveDir + remoteFile.RelativePath));
                        var response = await Services.GetFile(selectedGame, remoteFile.RelativePath);
                        var byteArr = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(gameSaveDir + remoteFile.RelativePath, byteArr);
                    }
                    else if (remoteFile.LastWriteTime > localFile?.LastWriteTime)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(gameSaveDir + remoteFile.RelativePath));
                        var response = await Services.GetFile(selectedGame, remoteFile.RelativePath);
                        var byteArr = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(gameSaveDir + remoteFile.RelativePath, byteArr);
                    }
                }

                var strLocalFiles = Directory.GetFiles(gameSaveDir, "*", SearchOption.AllDirectories).ToList();
                
                foreach (var strLocalFile in strLocalFiles)
                {
                    var localFile = new FileInfo(strLocalFile);
                    var remoteFile = remoteGame.FileList.Find(gfi => gfi.FileName == localFile.Name);
                    
                    if (remoteFile == null)
                    {
                        if (!selectedGame.FileList.Exists(gfi => gfi.FileName == localFile.Name))
                        {
                            selectedGame.FileList.Add(new GameFileInfo()
                            {
                                FileName = localFile.Name,
                                RelativePath = strLocalFile.Replace(gameSaveDir, "\\"),
                                LastWriteTime = Utilities.RoundDateTime(localFile.LastWriteTime)
                            });
                        }
                        else
                        {
                            selectedGame.FileList.Find(gfi => gfi.FileName == localFile.Name).LastWriteTime = Utilities.RoundDateTime(localFile.LastWriteTime);
                        }
                        var response = await Services.UploadFile(selectedGame, strLocalFile, strLocalFile.Replace(gameSaveDir, "\\"));
                        if (!response)
                        {
                            selectedGame = JsonHelper.Decode<SyncedGame>(gameTempBackup);
                            Utilities.ShowStatus("There was a problem uploading file " + localFile.Name + ".", Colors.Red);
                            return;
                        }
                    }
                    else if (localFile.LastWriteTime > remoteFile.LastWriteTime)
                    {
                        if (!selectedGame.FileList.Exists(gfi => gfi.FileName == localFile.Name))
                        {
                            selectedGame.FileList.Add(new GameFileInfo()
                            {
                                FileName = localFile.Name,
                                RelativePath = strLocalFile.Replace(gameSaveDir, "\\"),
                                LastWriteTime = Utilities.RoundDateTime(localFile.LastWriteTime)
                            });
                        }
                        else
                        {
                            selectedGame.FileList.Find(gfi => gfi.FileName == localFile.Name).LastWriteTime = Utilities.RoundDateTime(localFile.LastWriteTime);
                        }
   
                        var response = await Services.UploadFile(selectedGame, strLocalFile, strLocalFile.Replace(gameSaveDir, "\\"));
                        if (!response)
                        {
                            selectedGame = JsonHelper.Decode<SyncedGame>(gameTempBackup);
                            Utilities.ShowStatus("There was a problem uploading file " + localFile.Name + ".", Colors.Red);
                            return;
                        }
                    }
                }

                var updated = Utilities.RoundDateTime(DateTime.Now);
                selectedGame.LastLocalSync = updated;
                selectedGame.LastServerSync = updated;
                selectedGame.Status = "✔";
                selectedGame.StatusDetails = "This game is up-to-date.";
                long size = 0;
                foreach (var fi in new DirectoryInfo(gameSaveDir).GetFiles("*", SearchOption.AllDirectories))
                {
                    size += fi.Length;
                }
                selectedGame.StorageUse = size / 1024 / 1024;
                Utilities.ShowStatus("Syncing changes [" + (i + 1).ToString() + " of " + totalGames + "]...", Colors.Green);
                
                var response2 = await Services.SyncGame(selectedGame);
                if (response2)
                {
                    Utilities.ShowStatus("Game synced successfully.", Colors.Green);
                    dataSyncedGames.Items.Refresh();
                }
                else
                {
                    selectedGame = JsonHelper.Decode<SyncedGame>(gameTempBackup);
                    Utilities.ShowStatus("There was a problem syncing the game.", Colors.Red);
                }
            }
            await RetrieveAccount();
            tabMain.IsEnabled = true;
            Settings.Current.Save();
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
                ss.FileList.Add(new GameFileInfo()
                {
                    FileName = fileInfo.Name,
                    LastWriteTime = fileInfo.LastWriteTime,
                    RelativePath = fileInfo.FullName.Replace(contentPath, "\\")
                });
                size += fileInfo.Length;
                var result = await Services.UploadFile(ss, fileInfo.FullName, fileInfo.FullName.Replace(contentPath, "\\"));
                if (!result)
                {
                    Utilities.ShowStatus("Unable to add game.", Colors.Red);
                    tabMain.IsEnabled = true;
                    return;
                }
            }
            ss.StorageUse = size / 1024 / 1024;
            var response = await Services.AddGame(ss);
            if (response)
            {
                Settings.Current.AccountInfo.Games.Add(ss);
                await AnalyzeChanges();
                dataSyncedGames.Items.Refresh();
                tabMain.SelectedIndex = 0;
                Settings.Current.Save();
                Utilities.ShowStatus("Game added and synced.", Colors.Green);
                if (stackNoGames.Visibility == Visibility.Visible)
                {
                    stackNoGames.Visibility = Visibility.Collapsed;
                }
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
                Settings.Current.AccountInfo.AccountName = Username;
                Settings.Current.AccountInfo.Password = Password;
                var responseCheckAccount = await Services.CheckAccount();
                if (!responseCheckAccount.IsSuccessStatusCode)
                {
                    MessageBox.Show("The server cannot be reached.  Please check your internet connection.", "Network Unavailable", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var strResponseCheckAccount = await responseCheckAccount.Content.ReadAsStringAsync();
                if (strResponseCheckAccount == "false")
                {
                    buttonLogIn.IsEnabled = true;
                    throw new Exception("Incorrect username or password.  Try again or create a new account.");
                }
                var response = await responseCheckAccount.Content.ReadAsStringAsync();
                Guid authCode;
                if (Guid.TryParse(strResponseCheckAccount, out authCode))
                {
                    SettingsTemp.Current.AuthenticationCode = strResponseCheckAccount;
                    if (Settings.Current.RememberAccount)
                    {
                        Settings.Current.LastUser = textAccountName.Text;
                        Settings.Current.LastPassword = passPassword.Password;
                    }
                    gridLogIn.Visibility = Visibility.Collapsed;
                    gridNewAccount.Visibility = Visibility.Collapsed;
                    this.Width = 500;
                    this.MinWidth = 500;
                    this.Left -= 75;
                    if (this.Left < 0)
                    {
                        this.Left = 0;
                    }
                    gridGames.Visibility = Visibility.Visible;
                    Settings.Current.LoadUser(Settings.Current.AccountInfo.AccountName);
                    Utilities.ShowStatus("Retrieving account...", Colors.Green);
                    await AnalyzeChanges();
                    Utilities.ShowStatus("Done.", Colors.Green);
                    if (Settings.Current.AccountInfo.Games.Count == 0)
                    {
                        stackNoGames.Visibility = Visibility.Visible;
                    }
                    menuAccount.IsEnabled = true;
                    gridTitle.Visibility = Visibility.Collapsed;
                    await Task.Delay(100);
                    gridAccountInfo.Visibility = Visibility.Visible;
                    await Task.Delay(100);
                }
                else
                {
                    
                    throw new Exception("Unknown error.");
                }
            }
            catch (Exception ex)
            {
                Utilities.ShowStatus("Error: " + ex.Message, Colors.Red);
            }
            finally
            {
                Settings.Current.Save();
                buttonLogIn.IsEnabled = true;
            }
        }
        public async Task RetrieveAccount()
        {
            var responseGetAccount = await Services.GetAccount();
            var strResponseGetAccount = await responseGetAccount.Content.ReadAsStringAsync();
            SettingsTemp.Current.RemoteAccount = JsonHelper.Decode<AccountInfo>(strResponseGetAccount);
            Settings.Current.AccountInfo.StorageUsed = SettingsTemp.Current.RemoteAccount.StorageUsed;
            Settings.Current.AccountInfo.StorageTotal = SettingsTemp.Current.RemoteAccount.StorageTotal;
            Settings.Current.AccountInfo.IsSubscriber = SettingsTemp.Current.RemoteAccount.IsSubscriber;
            Settings.Current.AccountInfo.SubscriberID = SettingsTemp.Current.RemoteAccount.SubscriberID;
            Settings.Current.AccountInfo.LastPayment = SettingsTemp.Current.RemoteAccount.LastPayment;
            Settings.Current.AccountInfo.IsEmailEnabled = SettingsTemp.Current.RemoteAccount.IsEmailEnabled;
            Settings.Current.AccountInfo.IsQuestionEnabled = SettingsTemp.Current.RemoteAccount.IsQuestionEnabled;
            Settings.Current.AccountInfo.IsMachineGUIDEnabled = SettingsTemp.Current.RemoteAccount.IsMachineGUIDEnabled;
            Settings.Current.AccountInfo.Email = SettingsTemp.Current.RemoteAccount.Email;
            Settings.Current.AccountInfo.ChallengeQuestion = SettingsTemp.Current.RemoteAccount.ChallengeQuestion;
            Settings.Current.AccountInfo.ChallengeResponse = SettingsTemp.Current.RemoteAccount.ChallengeResponse;
            Settings.Current.Save();
            if (Settings.Current.AccountInfo.IsSubscriber)
            {
                textGetMoreStorage.Visibility = Visibility.Collapsed;
            }
            else
            {
                textGetMoreStorage.Visibility = Visibility.Visible;
            }
        }
        public async Task AnalyzeChanges()
        {
            if (SettingsTemp.Current.BypassAnalysis)
            {
                return;
            }
            Utilities.ShowStatus("Analyzing remote changes...", Colors.Green);
            await RetrieveAccount();
            
            foreach (var remoteGame in SettingsTemp.Current.RemoteAccount.Games)
            {
                var localGame = Settings.Current.AccountInfo.Games.Find(sg => sg.Name == remoteGame.Name);
                if (localGame == null)
                {
                    Settings.Current.AccountInfo.Games.Add(new SyncedGame()
                    {
                        Name = remoteGame.Name,
                        Path = remoteGame.Path,
                        LastServerSync = remoteGame.LastServerSync,
                        Status = "☁⬇",
                        StatusDetails = "Changes are available for download.",
                    });
                }
                else
                {
                    localGame.StorageUse = remoteGame.StorageUse;
                    if ( localGame.LastLocalSync != remoteGame.LastServerSync)
                    {
                        localGame.Status = "☁⬇";
                        localGame.StatusDetails = "Changes are available for download.";
                    }
                    foreach (var file in remoteGame.FileList)
                    {
                        if (!File.Exists(Utilities.ResolveEnvironmentVariables(remoteGame.Path) + "\\" + file.RelativePath))
                        {
                            localGame.Status = "☁⬇";
                            localGame.StatusDetails = "Changes are available for download.";
                        }
                    }
                }
            }

            Utilities.ShowStatus("Analyzing local changes...", Colors.Green);
            var listRemove = new List<SyncedGame>();
            foreach (var game in Settings.Current.AccountInfo.Games)
            {
                if (!SettingsTemp.Current.RemoteAccount.Games.Exists(sg => sg.Name == game.Name))
                {
                    listRemove.Add(game);
                    continue;
                }
                if (game.Status == "⛔" || game.Status == "✖" || game.Status == "❓" || game.Status == "⚠") 
                {
                    continue;
                }
                
                foreach (var file in Directory.GetFiles(Utilities.ResolveEnvironmentVariables(game.Path), "*", SearchOption.AllDirectories))
                {
                    var lastWriteTime = new FileInfo(file).LastWriteTime;
                    var creationTime = File.GetCreationTime(file);
                    if (lastWriteTime > game.LastLocalSync || creationTime > game.LastLocalSync)
                    {
                        if (game.Status.Contains("☁⬇"))
                        {
                            game.Status += "⬆";
                            game.StatusDetails = "Changes are available for both download and upload.";
                        }
                        else
                        {
                            game.Status = "☁⬆";
                            game.StatusDetails = "Changes are available for upload.";
                        }
                    }
                }
                if (game.Status == "✔")
                {
                    game.StatusDetails = "This game is up-to-date.";
                }
            }
            foreach (var game in listRemove)
            {
                Settings.Current.AccountInfo.Games.Remove(game);
            }
            Settings.Current.Save();
            dataSyncedGames.Items.Refresh();
            Utilities.ShowStatus("", Colors.Green);
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
                    await webClient.DownloadFileTaskAsync(new Uri("http://translucency.info/Downloads/Game Cloud.exe"), strFilePath);
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
            var strFiles = Directory.GetFiles(SettingsTemp.AppDataFolder + @"Backups\", "*", SearchOption.AllDirectories);
            foreach (var file in strFiles)
            {
                var fi = new FileInfo(file);
                listFiles.Add(fi);
                totalSize += fi.Length;
            }
            listFiles.Sort((FileInfo a, FileInfo b) => a.LastWriteTime.CompareTo(b.LastWriteTime));
            while (totalSize / 1024 / 1024 > Settings.Current.MaximumBackupSize || listFiles.Count > Settings.Current.MaximumBackupCount)
            {
                totalSize -= listFiles[0].Length;
                listFiles.RemoveAt(0);
            }
        }
        public void WriteToLog(string Message)
        {
            Directory.CreateDirectory(SettingsTemp.AppDataFolder);
            File.AppendAllText(SettingsTemp.AppDataFolder + @"ErrorLog.txt", DateTime.Now.ToString() + "\t" + Message + "\t" + Environment.NewLine);
        }

        #endregion Helper Methods
    }
}
