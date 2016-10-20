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
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }
        public string Version
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        private void hyperWebsite_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://translucency.info");
        }

        private void hyperChangeLog_Click(object sender, RoutedEventArgs e)
        {
            var win = new ChangeLog();
            win.Owner = App.Current.MainWindow;
            this.Close();
            win.ShowDialog();
        }

        private void buttonLicense_Click(object sender, RoutedEventArgs e)
        {
            var win = new LicenseInfo();
            win.Owner = this;
            win.ShowDialog();
        }
    }
}
