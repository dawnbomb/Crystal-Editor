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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Crystal_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Helloq() 
        {
        
        }
                

        
        private void BetaLabel_MouseMove(object sender, MouseEventArgs e)
        {          

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var label = (Label)sender;
                var grid = (Grid)label.Parent;
                var currentPosition = e.GetPosition(grid);
                var minimumDistance = (SystemParameters.MinimumHorizontalDragDistance + SystemParameters.MinimumVerticalDragDistance) / 2;

                if (Math.Sqrt(Math.Pow(currentPosition.X, 2) + Math.Pow(currentPosition.Y, 2)) >= minimumDistance)
                {                    
                    var data = new DataObject("myFormat", grid);
                    DragDrop.DoDragDrop(grid, data, DragDropEffects.Move);                    
                }
                label.ReleaseMouseCapture();
            }



        }
                

        private void Dock_Drop(object sender, DragEventArgs e)
        {            
            if (e.Data.GetDataPresent("myFormat"))
            {
                var grid = (Grid)e.Data.GetData("myFormat");
                var sourcePanel = (DockPanel)grid.Parent;
                sourcePanel.Children.Remove(grid);

                var targetPanel = (DockPanel)sender;
                targetPanel.Children.Insert(0, grid);
            }
        }
                



        


























        

















    }
}
