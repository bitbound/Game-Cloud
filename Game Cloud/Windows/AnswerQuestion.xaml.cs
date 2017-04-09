using Game_Cloud.Models;
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
    /// Interaction logic for AnswerQuestion.xaml
    /// </summary>
    public partial class AnswerQuestion : Window
    {
        public Question TheQuestion { get; set; }
        public AnswerQuestion()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            TheQuestion.AnsweredOn = DateTime.Now;
            TheQuestion.IsAnswered = "Yes";
            TheQuestion.Reply += $"--------------------{Environment.NewLine}Reply from {AccountInfo.Current.AccountName} on {DateTime.Now.ToString()}:" + Environment.NewLine + Environment.NewLine + textResponse.Text + Environment.NewLine + "--------------------" + Environment.NewLine + Environment.NewLine;
            var response = await Services.UpdateHelpRequest(TheQuestion);
            var strResponse = await response.Content.ReadAsStringAsync();
            if (strResponse == "true")
            {
                MessageBox.Show("Reply posted successfully.  Thanks for helping out!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("There was an error posting the reply.  Please try again or send a bug report.", "Posting Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
