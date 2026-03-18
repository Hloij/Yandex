using ScottPlot;
using ScottPlot.Palettes;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using System.Text;
using System.Text.RegularExpressions;
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
using Yandex.WorkerClass;
using System.Timers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Yandex.Wind
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StatisticYandexCountry statisticYandexCountry;
        bool zero=false;
       
        System.Timers.Timer timer1 = new();
        string ttime = "";
        public MainWindow()
        {
            InitializeComponent();
            Plot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("3C3C3C");//серый задник
            Plot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("3C3C3C");//серый внешний задник
            Plot.Plot.Axes.Color(ScottPlot.Color.FromHex("FF8C00"));

            Year.Text = DateTime.Now.Year.ToString();
            Month.SelectedIndex = DateTime.Now.Month - 1;

            long vs = (WorkWithBD<AdvertisingStreams>.Read(new()).Result).Where(p => p.Name == "Saite").FirstOrDefault().CountryOptionsId;
            // Устанавливаем интервал срабатывания таймера (например, каждые 5 секунд)
            var vs2 = WorkWithBD<CountryOprions>.Read(new()).Result;
            Task.Delay(1000).Wait();
            long vs3 = (vs2).Where(p => p.Id == (vs)).FirstOrDefault().Time *60;
          
            time.Text = ((vs2).Where(p => p.Id == (vs)).First().Time * 60).ToString (); // Интервал в миллисекундах
            ttime = ((vs2).Where(p => p.Id == (vs)).First().Time * 60).ToString();
            // Подписываем событие Elapsed (происходит каждый интервал)
       

            
            timer1.Interval = 1000; // Интервал в миллисекундах

            // Подписываем событие Elapsed (происходит каждый интервал)
            timer1.Elapsed += Time;

            // Включаем таймер
            timer1.Enabled = true;
          
           
           
        }

        private void Time(object? sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                time.Text=(Convert.ToInt32(time.Text)-1).ToString(); 
                if(time.Text =="0")
                {
                    time.Text = ttime;
                    Start();
                }
            });
        }

        private void Start(object? sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Start();
            });
        }
        
        bool can = false;
        void Start()
        {
            statisticYandexCountry = new StatisticYandexCountry("Saite", zero);
            statisticYandexCountry.Start().Wait();
            (SumCost.Text, SumPrice.Text, SumYea.Text, PnR.Text) = GetRezult(statisticYandexCountry.StatCountryNow.Sum(p => p.Dohod), statisticYandexCountry.StatCountryNow.Sum(p => p.Rashod));

            RaznicaStat.ItemsSource = statisticYandexCountry.StatCountryNow;
            RaznicaStatDevice.ItemsSource = statisticYandexCountry.StatDeviseNow;
            SravnenieDevice.ItemsSource = statisticYandexCountry.SravnenieDevice;
            Sravnenie.ItemsSource = statisticYandexCountry.SravnenieCountry.ToList();

            Upper.ItemsSource = statisticYandexCountry.Up.ToList();
            Down.ItemsSource = statisticYandexCountry.Down.ToList();
            Nuller.ItemsSource = statisticYandexCountry.Null.ToList();
            viborkaTEST.IsEnabled = true;
           
            //InPlot();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }
        private void RaznicaStat_SelectedCellsChanged(object sender, DataGridCellEditEndingEventArgs e)
        {

            var send = sender as DataGrid;
            var changeditem = e.Row.Item as YandexStat;
            statisticYandexCountry.ChangeKoefRukami(changeditem);


        }
        private void RaznicaStatВDevice_SelectedCellsChanged(object sender, DataGridCellEditEndingEventArgs e)
        {
            var send = sender as DataGrid;
            var changeditem = e.Row.Item as YandexStat;


            statisticYandexCountry.ChangeKoefRukami(changeditem);

        }
        private void Upper_Selected(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var obj = statisticYandexCountry.StatCountryNow.Where(p => p.Name == e.AddedItems[0]).First();

                int i = RaznicaStat.Items.IndexOf(obj);
                RaznicaStat.SelectedIndex = i;
                RaznicaStat.ScrollIntoView(obj);
            }
            catch
            {

            }

            try
            {
                var obj1 = statisticYandexCountry.SravnenieCountry.Where(p => p.Name == e.AddedItems[0]).First();

                int i = Sravnenie.Items.IndexOf(obj1);
                Sravnenie.SelectedIndex = i;
                Sravnenie.ScrollIntoView(obj1);
            }
            catch { }
            try
            {
                var obj = statisticYandexCountry.StatDeviseNow.Where(p => p.Name == e.AddedItems[0]).First();

                int i = RaznicaStatDevice.Items.IndexOf(obj);
                RaznicaStatDevice.SelectedIndex = i;
                RaznicaStatDevice.ScrollIntoView(obj);
            }
            catch
            {

            }

            try
            {
                var obj1 = statisticYandexCountry.SravnenieDevice.Where(p => p.Name == e.AddedItems[0]).First();

                int i = SravnenieDevice.Items.IndexOf(obj1);
                SravnenieDevice.SelectedIndex = i;
                SravnenieDevice.ScrollIntoView(obj1);
            }
            catch { }

        }
        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (can)
            {
                var pos = e.GetPosition(sender as WpfPlot);
                float X = (float)pos.X;
                float Y = (float)pos.Y;
                // determine where the mouse is and get the nearest point
                Pixel mousePixel = new(X, Y);
                Coordinates mouseLocation = Plot.Plot.GetCoordinates(mousePixel);
                List<DataPoint> dataPoints = new List<DataPoint>();

                DataPoint nearest = scatter1.Data.GetNearestX(mouseLocation, Plot.Plot.LastRender);
                dataPoints.Add(nearest);


                double today = 0;

                for (int i = 0; i < dataPoints.Count; i++)
                {
                    if (dataPoints[i].IsReal)
                    {
                        crosshair1.IsVisible = true;
                        crosshair1.Position = dataPoints[i].Coordinates;

                        today = dataPoints[i].Y;




                    }
                }
                Plot.Plot.Title($"Доход : {today:0.##} Всего за месяц : {month}");
                Plot.Refresh();
            }
        }
        void InPlot()
        {
            month = 0;
            Plot.Plot.Clear();
            List<StatCountryDevice> StatAll = WorkWithBD<StatCountryDevice>.Read(new StatCountryDevice()).Result;
            List<YandexStat> LastStat = new List<YandexStat>();

            for (int i = 0; i < StatAll.Count; i++)
            {
                string pattern = @"\d{2}.\d{2}.\d{4}-\d{2}.\d{2}.\d{4}$";

                for (int j = i + 1; j < StatAll.Count; j++)
                {

                    if (StatAll[i].Name == StatAll[j].Name && StatAll[i].Date == StatAll[j].Date)
                    {

                        DateTime a1 = Convert.ToDateTime(StatAll[i].Date);
                        DateTime a2 = Convert.ToDateTime(StatAll[j].Date);
                        if (a1.Hour == a2.Hour)
                        {
                            StatAll.Remove(StatAll[i]);

                            i--;
                        }

                        break;
                    }

                }


            }

            int currentYear = Convert.ToInt32(Year.Text);
            int currentMonth = Month.SelectedIndex + 1;

            // Определяем количество дней в текущем месяце
            int daysInCurrentMonth = DateTime.DaysInMonth(currentYear, currentMonth);

            // Создаем массив дней
            DateTime[] daysArray = new DateTime[daysInCurrentMonth];
            double[] dohod = new double[daysInCurrentMonth];
            for (int i = 0; i < daysInCurrentMonth; i++)
            {
                DateTime day = new DateTime(currentYear, currentMonth, i + 1);
                daysArray[i] = day;
            }

            bool a = true;

            for (int h = 0; h < daysArray.Length; h++)
            {
                List<YandexStat> stata = new();
                List<YandexStat> statmonth = new List<YandexStat>();

                //все записи за этот день
                foreach (var item in StatAll)
                {
                    DateTime a1 = Convert.ToDateTime(item.Date);
                    if (a1 == daysArray[h])
                    {
                        statmonth.Add(new YandexStat() + item);
                    }
                }

                //выкидываем все кроме последне записи
                foreach (var item in statmonth)
                {
                    bool was = false;
                    foreach (var stat in stata)
                    {
                        //if (item.WhatPotok != stat.WhatPotok)

                        if (item.Name == stat.Name)
                        {

                            DateTime a1 = Convert.ToDateTime(item.Date);
                            DateTime a2 = Convert.ToDateTime(stat.Date);
                            if (a1 > a2)
                            {
                                int i = stata.IndexOf(stat);
                                stata[i] = item;
                            }
                            was = true;
                            break;
                        }


                    }
                    if (stata.Count == 0) { stata.Add(item); was = true; }
                    if (was == false)
                    {
                        stata.Add(item);
                    }
                }
                for (int j = 0; j < stata.Count; j++)
                {
                    dohod[h] += stata[j].Dohod;
                }
            }


            Plot.Plot.Add.Scatter(daysArray, dohod, color: ScottPlot.Color.FromHex("FF8C00")); //оранжевый грфик за сегодня
            //Plot.Plot.Axes.Bottom.SetTicks(dohod, daysArray);
            //scatter1 = Plot.Plot.Add.Scatter(daysArray, dohod);
            var axis = Plot.Plot.Axes.DateTimeTicksBottom();
            static string CustomFormatter(DateTime dt)
            {
                bool isMidnight = dt is { Hour: 0, Minute: 0, Second: 0 };
                return isMidnight
                    ? DateOnly.FromDateTime(dt).ToString()
                    : TimeOnly.FromDateTime(dt).ToString();
            }
            var tickGen = (ScottPlot.TickGenerators.DateTimeAutomatic)axis.TickGenerator;
            tickGen.LabelFormatter = CustomFormatter;

            scatter1 = Plot.Plot.Add.Scatter(daysArray, dohod);



            crosshair1 = Plot.Plot.Add.Crosshair(0, 0);
            crosshair1.IsVisible = false;
            crosshair1.MarkerShape = MarkerShape.OpenCircle;
            crosshair1.MarkerSize = 15;
            //crosshair1 = Plot.Plot.Add.Crosshair(0, 0);

            for (int i = 0; i < dohod.Length; i++)
            {
                month += dohod[i];
            }
            //Plot.Plot.Axes.AutoScale();
            Plot.Plot.Axes.Color(ScottPlot.Color.FromHex("FF8C00"));
            Plot.Refresh();
            can = true;
        }
        double month = 0;
        Scatter scatter1;
        Crosshair crosshair1;

        private void AutoPil_Checked(object sender, RoutedEventArgs e)
        {
            if (Zero.IsChecked == true) zero = true;
            else zero = false;
        }

        public static (string SumCost, string SumPrice, string SumYea, string PnR) GetRezult(double infoOneprofit, double priceAdFox)//нахождение всех результатов
        {
            double sumCost = infoOneprofit;

            double priceCost = priceAdFox;                    //результат общего расхода
            double sumYea = sumCost - priceAdFox;        //результат чистой прибыли
            double pnr = Math.Round(sumYea / priceAdFox, 2);  //результат коэфицента результативности
            return (Math.Round(sumCost, 2).ToString(), Math.Round(priceCost, 2).ToString(), Math.Round(sumYea, 2).ToString(), Math.Round(pnr, 2).ToString());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Start();
            timer1.Start();
        }
    }
}