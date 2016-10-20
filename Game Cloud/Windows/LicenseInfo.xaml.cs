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
    /// Interaction logic for LicenseInfo.xaml
    /// </summary>
    public partial class LicenseInfo : Window
    {
        public LicenseInfo()
        {
            InitializeComponent();
        }

        private void buttonCopy_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            foreach (var inline in textInfo.Inlines)
            {
                if (inline is Run)
                {
                    text += (inline as Run).Text;
                }
                else if (inline is LineBreak)
                {
                    text += Environment.NewLine;
                }
            }
            Clipboard.SetText(text);
        }
    }
}
