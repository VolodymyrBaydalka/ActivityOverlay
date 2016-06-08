using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace ActivitySample
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

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            Activity.EnqueueActivity(async (t) => {
                await Task.Delay(1000, t);
                this.Title = "Error";
                throw new Exception("Error message");
            }, "Sleep", "Sleeping...", true, true, true);
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            Activity.EnqueueActivity(async (t) => {
                await Task.Delay(1000, t);
                this.Title = "Success";
            }, "Sleep", "Sleeping...", true, true, false, true);
        }
    }
}
