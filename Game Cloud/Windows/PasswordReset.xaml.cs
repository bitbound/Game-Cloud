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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Game_Cloud.Windows
{
    /// <summary>
    /// Interaction logic for PasswordReset.xaml
    /// </summary>
    public partial class PasswordReset : Window
    {
        private string accountName { get; set; }
        public PasswordReset(string AccountName)
        {
            InitializeComponent();
            accountName = AccountName;
        }

        private async void buttonEmail_Click(object sender, RoutedEventArgs e)
        {
            var response = await Services.RecoverPassword(accountName, "email", null);
            if (response == null)
            {
                return;
            }
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Password reset successful.  Please check your email for further instructions.  If the email doesn't arrive within a couple minutes, check your junk/spam folder.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("There was an unknown error.  Please try again or submit a bug report.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.Close();
        }

        private void buttonQuestion_Click(object sender, RoutedEventArgs e)
        {
            stackMain.Visibility = Visibility.Collapsed;
            stackQA.Visibility = Visibility.Visible;
        }

        private async void buttonGUID_Click(object sender, RoutedEventArgs e)
        {
            RegistryKey key;
            if (Environment.Is64BitOperatingSystem)
            {
                RegistryKey regKeyBase = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                key = regKeyBase.OpenSubKey(@"Software\Microsoft\Cryptography", Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
                regKeyBase.Close();
            }
            else
            {
                key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Cryptography", Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey);
            }
            var value = key.GetValue("MachineGuid").ToString();
            key.Close();
            var response = await Services.RecoverPassword(accountName, "guid", value);
            if (response == null)
            {
                return;
            }
            var tempPass = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                if (tempPass == "false")
                {
                    MessageBox.Show("The machine ID of your current computer does not match the one on record.", "ID Mismatch", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                Clipboard.SetText(tempPass);
                await Task.Run(new Action(()=> { MessageBox.Show("Password reset successful.  Your temporary password is \"" + tempPass + "\" (minus the quotes) and has been *copied to your clipboard automatically*.  Please log in and change your password immediately.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }));
            }
            else
            {
                MessageBox.Show("There was an unknown error.  Please try again or submit a bug report.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.Close();
        }

        private async void buttonSubmitAnswer_Click(object sender, RoutedEventArgs e)
        {
            var response = await Services.RecoverPassword(accountName, "question", textAnswer.Text);
            if (response == null)
            {
                return;
            }
            var tempPass = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                if (tempPass == "false")
                {
                    MessageBox.Show("Your answer does not match the one on record.", "Incorrect Answer", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                Clipboard.SetText(tempPass);
                await Task.Run(new Action(() => { MessageBox.Show("Password reset successful.  Your temporary password is \"" + tempPass + "\" (minus the quotes) and has been *copied to your clipboard automatically*.  Please log in and change your password immediately.", "Success", MessageBoxButton.OK, MessageBoxImage.Information); }));
            }
            else
            {
                MessageBox.Show("There was an unknown error.  Please try again or submit a bug report.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.Close();
        }
    }
}
