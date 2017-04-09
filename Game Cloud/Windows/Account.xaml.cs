using Game_Cloud.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
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
            this.DataContext = AccountInfo.Current;
        }

        private async void buttonSaveEmail_Click(object sender, RoutedEventArgs e)
        {
            var strAccountBackup = Json.Encode(AccountInfo.Current);
            AccountInfo.Current.Email = textEmail.Text;
            var result = await Services.UpdateRecoveryOptions();
            if (result == null)
            {
                return;
            }
            if (result.IsSuccessStatusCode)
            {
                MessageBox.Show("Email has been saved.  Please verify it was entered correctly.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var accountBackup = Json.Decode<AccountInfo>(strAccountBackup);
                AccountInfo.Current.Email = accountBackup.Email;
                MessageBox.Show("The update failed.  Unknown error.  Please try again or send a bug report.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            Settings.Current.Save();
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
            var strAccountBackup = Json.Encode(AccountInfo.Current);
            if (checkEmailReset.IsChecked == true)
            {
                AccountInfo.Current.IsEmailEnabled = true;
                AccountInfo.Current.Email = textEmail.Text;
            }
            else
            {
                AccountInfo.Current.IsEmailEnabled = false;
                AccountInfo.Current.Email = "";
            }
            if (checkChallenge.IsChecked == true)
            {
                AccountInfo.Current.IsQuestionEnabled = true;
                AccountInfo.Current.ChallengeQuestion = textQuestion.Text;
                AccountInfo.Current.ChallengeResponse = textAnswer.Text;
            }
            else
            {
                AccountInfo.Current.IsQuestionEnabled = false;
                AccountInfo.Current.ChallengeQuestion = "";
                AccountInfo.Current.ChallengeResponse = "";
            }
            if (checkMachineGuid.IsChecked == true)
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    RegistryKey regKeyBase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    var key = regKeyBase.OpenSubKey(@"Software\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
                    AccountInfo.Current.MachineGUID = key.GetValue("MachineGuid").ToString();
                    AccountInfo.Current.IsMachineGUIDEnabled = true;
                    key.Close();
                    regKeyBase.Close();
                }
                else
                {
                    var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Cryptography", RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
                    AccountInfo.Current.MachineGUID = key.GetValue("MachineGuid").ToString();
                    AccountInfo.Current.IsMachineGUIDEnabled = true;
                    key.Close();
                }
            }
            else
            {
                AccountInfo.Current.IsMachineGUIDEnabled = false;
                AccountInfo.Current.MachineGUID = null;
            }
            var result = await Services.UpdateRecoveryOptions();
            if (result == null)
            {
                return;
            }
            if (result.IsSuccessStatusCode)
            {
                MessageBox.Show("Account recovery options have been updated.", "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var accountBackup = Json.Decode<AccountInfo>(strAccountBackup);
                AccountInfo.Current.IsEmailEnabled = accountBackup.IsEmailEnabled;
                AccountInfo.Current.Email = accountBackup.Email;
                AccountInfo.Current.IsQuestionEnabled = accountBackup.IsQuestionEnabled;
                AccountInfo.Current.ChallengeQuestion = accountBackup.ChallengeQuestion;
                AccountInfo.Current.ChallengeResponse = accountBackup.ChallengeResponse;
                AccountInfo.Current.IsMachineGUIDEnabled = accountBackup.IsMachineGUIDEnabled;
                AccountInfo.Current.MachineGUID = accountBackup.MachineGUID;
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
            var hashedPassword = Crypto.Hash(passNewPassword.Password);
            var result = await Services.ChangePassword(hashedPassword);
            if (result.IsSuccessStatusCode)
            {
                if (await result.Content.ReadAsStringAsync() == "false")
                {
                    MessageBox.Show("Your stored current password is incorrect.  Log out and back in, then try again.", "Incorrect Password", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
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
            AccountInfo.Current.IsMachineGUIDEnabled = true;
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
