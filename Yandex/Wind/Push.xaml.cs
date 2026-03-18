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

namespace Yandex.Wind
{
    /// <summary>
    /// Логика взаимодействия для Push.xaml
    /// </summary>
    public partial class Push : Window
    {
        public Push(string text,string title)
        {
            InitializeComponent();
            tbText.Text = text;
            tbTitle.Text = title;
            this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Delay(300000).Wait();
            this.Close();
        }
        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
