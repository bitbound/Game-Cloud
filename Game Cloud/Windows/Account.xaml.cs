using Game_Cloud.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Game_Cloud.Windows
{
    /// <summary>
    /// Interaction logic for Account.xaml
    /// </summary>
    public partial class Account : Window
    {
        public Account()
        {
            InitializeComponent();
            this.DataContext = Settings.Current;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Current.AccountInfo.IsSubscriber)
            {
                textAccountType.Text = "Subscription Account";
                textActiveThrough.Text = Settings.Current.AccountInfo.LastPayment.AddMonths(1).ToString();
                textCancelSubscription.Visibility = Visibility.Visible;
                textSubscribeNow.Visibility = Visibility.Collapsed;
            }
            else
            {
                textAccountType.Text = "Free Account";
                textActiveThrough.Text = "N/A";
                textSubscribeNow.Visibility = Visibility.Visible;
                textCancelSubscription.Visibility = Visibility.Collapsed;
            }
        }

        private void hyperSubscribeNow_Click(object sender, RoutedEventArgs e)
        {
            var win = new Windows.Subscribe();
            win.Owner = App.Current.MainWindow;
            this.Close();
            win.ShowDialog();
        }

        private void hyperCancelSubscription_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://translucency.info/Pages/GameCloudCancel.cshtml");
        }

        private async void buttonSaveRecovery_Click(object sender, RoutedEventArgs e)
        {
            if (checkEmailReset.IsChecked == true)
            {
                if (textEmail.Text.Length == 0)
                {
                    MessageBox.Show("An email address is required.", "Email Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
            }
            if (checkChallenge.IsChecked == true)
            {
                if (textQuestion.Text.Length == 0 || textAnswer.Text.Length == 0)
                {
                    MessageBox.Show("A question and answer is required.", "Question/Answer Required", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
            }
            var strAccountBackup = JsonHelper.Encode(Settings.Current.AccountInfo);
            if (checkEmailReset.IsChecked == true)
            {
                MessageBox.Show("I don't validate email addresses in any way.  Please make sure your email address is entered correctly.", "Verify Email", MessageBoxButton.OK, MessageBoxImage.Information);
                Settings.Current.AccountInfo.IsEmailEnabled = true;
                Settings.Current.AccountInfo.Email = textEmail.Text;
            }
            else
            {
                Settings.Current.AccountInfo.IsEmailEnabled = false;
                Settings.Current.AccountInfo.Email = "";
            }
            if (checkChallenge.IsChecked == true)
            {
                Settings.Current.AccountInfo.IsQuestionEnabled = true;
                Settings.Current.AccountInfo.ChallengeQuestion = textQuestion.Text;
                Settings.Current.AccountInfo.ChallengeResponse = textAnswer.Text;
            }
            else
            {
                Settings.Current.AccountInfo.IsQuestionEnabled = false;
                Settings.Current.AccountInfo.ChallengeQuestion = "";
                Settings.Current.AccountInfo.ChallengeResponse = "";
            }
            if (checkMachineGuid.IsChecked == true)
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    RegistryKey regKeyBase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    var key = regKeyBase.OpenSubKey(@"Software\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
                    Settings.Current.AccountInfo.MachineGUID = key.GetValue("MachineGuid").ToString();
                    Settings.Current.AccountInfo.IsMachineGUIDEnabled = true;
                    key.Close();
                    regKeyBase.Close();
                }
                else
                {
                    var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
                    Settings.Current.AccountInfo.MachineGUID = key.GetValue("MachineGuid").ToString();
                    Settings.Current.AccountInfo.IsMachineGUIDEnabled = true;
                    key.Close();
                }
            }
            else
            {
                Settings.Current.AccountInfo.IsMachineGUIDEnabled = false;
                Settings.Current.AccountInfo.MachineGUID = null;
            }
            var result = await Services.UpdateRecoveryOptions();
            if (result.IsSuccessStatusCode)
            {
                MessageBox.Show("Account recovery options have been updated.", "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var accountBackup = JsonHelper.Decode<AccountInfo>(strAccountBackup);
                Settings.Current.AccountInfo.IsEmailEnabled = accountBackup.IsEmailEnabled;
                Settings.Current.AccountInfo.Email = accountBackup.Email;
                Settings.Current.AccountInfo.IsQuestionEnabled = accountBackup.IsQuestionEnabled;
                Settings.Current.AccountInfo.ChallengeQuestion = accountBackup.ChallengeQuestion;
                Settings.Current.AccountInfo.ChallengeResponse = accountBackup.ChallengeResponse;
                Settings.Current.AccountInfo.IsMachineGUIDEnabled = accountBackup.IsMachineGUIDEnabled;
                Settings.Current.AccountInfo.MachineGUID = accountBackup.MachineGUID;
                MessageBox.Show("The update failed.  Unknown error.  Please try again or send a bug report.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            Settings.Current.Save();
        }

        private async void buttonSavePassword_Click(object sender, RoutedEventArgs e)
        {            
            if (passNewPassword.Password != passConfirmPassword.Password)
            {
                MessageBox.Show("The supplied passwords don't match.", "Passwords Don't Match", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            var hashedPassword = HashHelper.HashString(passNewPassword.Password);
            var result = await Services.ChangePassword(hashedPassword);
            if (result.IsSuccessStatusCode)
            {
                if (await result.Content.ReadAsStringAsync() == "false")
                {
                    MessageBox.Show("Your stored current password is incorrect.  Log out and back in, then try again.", "Incorrect Password", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                Settings.Current.AccountInfo.Password = passNewPassword.Password;
                Settings.Current.Save();
                passNewPassword.Clear();
                passConfirmPassword.Clear();
                MessageBox.Show("Password changed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Password change failed.  Unknown error.  Please try again or send a bug report.", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private async void checkMachineGuid_Checked(object sender, RoutedEventArgs e)
        {
            RegistryKey regKeyBase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var key = regKeyBase.OpenSubKey(@"Software\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
            var value = key.GetValue("MachineGuid").ToString();
            Settings.Current.AccountInfo.IsMachineGUIDEnabled = true;
            key.Close();
            regKeyBase.Close();
            if (value == null)
            {
                checkMachineGuid.IsChecked = false;
                var tt = new ToolTip();
                tt.PlacementTarget = checkMachineGuid;
                tt.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                tt.Content = "The machine GUID is unavailable on this computer.";
                tt.IsOpen = true;
                await Task.Delay(1500);
                tt.BeginAnimation(OpacityProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(1)));
                await Task.Delay(1000);
                tt.IsOpen = false;
            }
        }
    }
}
