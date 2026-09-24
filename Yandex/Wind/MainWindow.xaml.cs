using ScottPlot;
using ScottPlot.Palettes;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Yandex.Model;
using Yandex.Model.ModelsBD;
using Yandex.Pagees;
using Yandex.WorkerClass;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Yandex.Wind
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
        Page[] page;
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < page.Length; i++)
            {
                if (page[i].Name == (sender as MenuItem).Header)
                {
                    Fr.NavigationService.Navigate(page[i]);
                    foreach (MenuItem item in Mn.Items)
                    {
                        if(item!= (sender as MenuItem))
                        {
                            item.Background = new MenuItem().Background;
                        }                        
                    }
                    (sender as MenuItem).Background = Brushes.Wheat;
                    return;
                }
            }
            Wind.Options options = new Options();
            options.Show();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var streams = WorkWithBD<AdvertisingStreams>.Read(new()).Result;
            page = new Page[streams.Count];
            int i = 0;
            foreach (var stream in streams)
            {
                MenuItem menuItem = new MenuItem();
                menuItem.Header = stream.Name;
                menuItem.Click += MenuItem_Click;
                Mn.Items.Add(menuItem);

                page[i] = new PageWithStat(stream.Name);
                i++;
            }
        }
    }
}