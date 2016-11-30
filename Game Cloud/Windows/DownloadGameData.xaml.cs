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
    /// Interaction logic for DownloadGameData.xaml
    /// </summary>
    public partial class DownloadGameData : Window
    {
        public SyncedGame SelectedGame { get; set; }
        public DownloadGameData()
        {
            InitializeComponent();
            this.DataContext = Settings.Current;
        }

        public static async Task<SyncedGame> SelectGame()
        {
            var win = new DownloadGameData();
            win.Owner = Options.Current;
            win.Show();
            while (win.IsVisible && win.SelectedGame == null)
            {
                await Task.Delay(5);
            }
            win.Close();
            return win?.SelectedGame;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            SelectedGame = (SyncedGame)listGames.SelectedItem;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
