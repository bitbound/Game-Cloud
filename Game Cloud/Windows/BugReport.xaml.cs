using Game_Cloud.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
    /// Interaction logic for BugReport.xaml
    /// </summary>
    public partial class BugReport : Window
    {
        public BugReport()
        {
            InitializeComponent();
            this.DataContext = AccountInfo.Current;
        }
        private async void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            if (textName.Text == "" || textMessage.Text == "")
            {
                MessageBox.Show("All fields are required.  Please fill them out first.", "Missing Fields", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            else
            {
                buttonSend.IsEnabled = false;
                try
                {
                    var client = new HttpClient();
                    var request = new MultipartFormDataContent();
                    var fi = new FileInfo(Utilities.AppDataFolder + "ErrorLog.txt");
                    var message = textMessage.Text;
                    if (fi.Exists)
                    {
                        request.Add(new StringContent(Convert.ToBase64String(File.ReadAllBytes(fi.FullName))), "Attachment");
                    }
                    request.Add(new StringContent("SendEmail"), "Command");
                    request.Add(new StringContent(textName.Text), "Name");
                    request.Add(new StringContent("Game Cloud Bug Report"), "Subject");
                    request.Add(new StringContent("us@invis.me"), "From");
                    request.Add(new StringContent(message), "Message");
                    var result = await client.PostAsync("https://invis.me/Services/GameCloud/", request);
                    var response = await result.Content.ReadAsStringAsync();
                    if (result.IsSuccessStatusCode)
                    {
                        textName.Text = "";
                        textMessage.Text = "";
                        MessageBox.Show("Bug reported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                    else if (response.Contains("The specified string is not in the form required for an e-mail address."))
                    {
                        MessageBox.Show("That doesn't look like a valid email addres.", "Invalid Email", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show("There was a problem sending the message.  Make sure you have an internet connection and try again.", "Error Sending Message", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
                catch
                {
                    MessageBox.Show("There was a problem sending the message.  Make sure you have an internet connection and try again.", "Error Sending Message", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                finally
                {
                    buttonSend.IsEnabled = true;
                }
            }
        }
        private void textMessage_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textMessage.Text == "Please describe what you were doing when the error occurred.")
            {
                textMessage.Text = "";
            }
            textMessage.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void textMessage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (textMessage.Text == "")
            {
                textMessage.Text = "Please describe what you were doing when the error occurred.";
            }
            textMessage.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void hyperLog_Click(object sender, RoutedEventArgs e)
        {
            var fi = new FileInfo(Utilities.AppDataFolder + "ErrorLog.txt");
            if (!fi.Exists)
            {
                fi.OpenWrite().Close();
            }
            System.Diagnostics.Process.Start(fi.FullName);
        }
    }
}
