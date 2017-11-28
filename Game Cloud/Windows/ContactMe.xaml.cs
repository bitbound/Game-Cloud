using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace Game_Cloud.Windows
{
    /// <summary>
    /// Interaction logic for ContactMe.xaml
    /// </summary>
    public partial class ContactMe : Window
    {
        public ContactMe()
        {
            InitializeComponent();
        }

        private async void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            if (textName.Text == "" || textEmail.Text == "" || textMessage.Text == "")
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
                    request.Add(new StringContent("SendEmail"), "Command");
                    request.Add(new StringContent(textName.Text), "Name");
                    request.Add(new StringContent("Message from Game Cloud"), "Subject");
                    request.Add(new StringContent(textEmail.Text), "From");
                    request.Add(new StringContent(textMessage.Text), "Message");
                    var result = await client.PostAsync("http://invis.me/Services/GameCloud/", request);
                    var response = await result.Content.ReadAsStringAsync();
                    if (result.IsSuccessStatusCode)
                    {
                        textName.Text = "";
                        textEmail.Text = "";
                        textMessage.Text = "";
                        MessageBox.Show("Message sent successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}
