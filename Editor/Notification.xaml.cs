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

namespace Crystal_Editor
{
    /// <summary>
    /// Interaction logic for Notification.xaml
    /// </summary>
    public partial class Notification : Window
    {
        public Notification(string Error)
        {
            InitializeComponent();

            MessageBox.Text = Error;
        }

        public void Example() 
        {
            string Error = "Example text " +
                "\n" +
                "\nFinal Line. ";
            Notification f2 = new(Error);
            f2.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            f2.ShowDialog();
            return;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
