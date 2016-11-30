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
    /// Interaction logic for Subscribe.xaml
    /// </summary>
    public partial class Subscribe : Window
    {
        public Subscribe()
        {
            InitializeComponent();
            DataContext = Settings.Current;
        }

        private void hyperPayPal_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://translucency.info/Pages/GameCloudSubscribe.cshtml");
        }

        private async void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            if (textProfileID.Text.Trim().Length == 0)
            {
                return;
            }
            var profileID = textProfileID.Text.Trim();
            var result = await Services.ApplySubscription(profileID);
            var response = await result.Content.ReadAsStringAsync();
            if (response == "true")
            {
                await MainWindow.Current.AnalyzeChanges();
                MessageBox.Show("Subscription benefits have been applied!  Thank you for your support.", "Subscription Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else if (response == "claimed")
            {
                MessageBox.Show("The supplied Profile ID has already been claimed.", "ID Claimed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                MessageBox.Show("A current subscription couldn't be found.  Please check the Profile ID and try again.  If you're getting this message in error, please contact me via the Feedback tab.", "Subscription Not Found", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
