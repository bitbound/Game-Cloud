using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public static Options Current { get; set; }
        public Options()
        {
            InitializeComponent();
            DataContext = Settings.Current;
            Current = this;
        }

        private void toggleBackups_Unchecked(object sender, RoutedEventArgs e)
        {
            toggleBackups.Content = "Off";
        }

        private void toggleBackups_Checked(object sender, RoutedEventArgs e)
        {
            toggleBackups.Content = "On";
        }
        private async void toggleHelpRequests_Checked(object sender, RoutedEventArgs e)
        {
            toggleHelpRequests.Content = "On";
            await MainWindow.Current.UpdateQuestions();
        }

        private void toggleHelpRequests_Unchecked(object sender, RoutedEventArgs e)
        {
            toggleHelpRequests.Content = "Off";
            MainWindow.Current.borderHelpRequestsAvailable.Visibility = Visibility.Collapsed;
        }
        private void buttonBrowseBackups_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Utilities.AppDataFolder + @"Backups\"))
            {
                System.Diagnostics.Process.Start(Utilities.AppDataFolder + @"Backups\");
            }
            else if (System.IO.Directory.Exists(Utilities.AppDataFolder + @""))
            {
                System.Diagnostics.Process.Start(Utilities.AppDataFolder + @"");
            }
            else
            {
                System.Diagnostics.Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            }
        }

        private void buttonRemoveGameCloud_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("This will remove all local Game Cloud files in Local AppData.  Do you want to continue?", "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Directory.Delete(Utilities.AppDataFolder, true);
                    MessageBox.Show("Game Cloud files have been removed.  The application will now close.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Temp.Current.Uninstall = true;
                    App.Current.Shutdown();
                }
                catch
                {
                    MessageBox.Show("The Game Cloud folder couldn't be deleted.  Make sure all files are closed and try again.", "Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private async void buttonDownload_Click(object sender, RoutedEventArgs e)
        {
            var selectedGame = await DownloadGameData.SelectGame();
            if (selectedGame == null)
            {
                return;
            }
            var filePicker = new Microsoft.Win32.SaveFileDialog();
            filePicker.AddExtension = true;
            filePicker.DefaultExt = ".zip";
            filePicker.Filter = "ZIP (*.zip)|*.zip";
            filePicker.Title = "Download Saved Game";
            var result = filePicker.ShowDialog();
            if (result != true)
            {
                return;
            }
            Utilities.ShowStatus("Downloading game files...", Colors.Green);
            var response = await Services.GetGame(selectedGame);
            if (response == null)
            {
                return;
            }
            var byteRemoteGame = await response.Content.ReadAsByteArrayAsync();
            File.WriteAllBytes(filePicker.FileName, byteRemoteGame);
            Utilities.ShowStatus("Done.", Colors.Green);
        }

        
    }
}
