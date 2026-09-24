using Newtonsoft.Json.Linq;
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
using Yandex.Model.ModelsBD;
using Yandex.WorkerClass;

namespace Yandex.Wind
{
    /// <summary>
    /// Логика взаимодействия для Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
        }
        List<AdvertisingCompany> adCompany;
        List<AdvertisingStreams> adStream;
        List<CountryOprions> OptionsCountry;
        List<DeviceOptions> OptionsDevice;
        List<UserOptions> UserOptions;


        public void CheckboxInPotoks(object sender, RoutedEventArgs e)
        {
            if ((e.Source as CheckBox).IsChecked == true)
            {
                cbWhatAdCompanys.Text += (e.Source as CheckBox).Content + ",";

            }
            else
            {
                cbWhatAdCompanys.Text = cbWhatAdCompanys.Text.Replace((e.Source as CheckBox).Content + ",", "");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            adCompany = WorkWithBD<AdvertisingCompany>.Read(new AdvertisingCompany()).Result;
            adStream = WorkWithBD<AdvertisingStreams>.Read(new AdvertisingStreams()).Result;
            OptionsCountry = WorkWithBD<CountryOprions>.Read(new CountryOprions()).Result;
            OptionsDevice = WorkWithBD<DeviceOptions>.Read(new DeviceOptions()).Result;
            UserOptions = WorkWithBD<UserOptions>.Read(new UserOptions()).Result;
            var vs = LogicalTreeHelper.GetChildren(tab);

            tbAdCompanyId.Text = "";
            tbAdCompanyName.Text = "";
            tbCountryOptionsCPMV.Text = "";
            tbCountryOptionsCPMV.Text = "";
            tbCountryOptionsDown.Text = "";
            tbCountryOptionsMax.Text = "";
            tbCountryOptionsMin.Text = "";
            tbCountryOptionsName.Text = "";
            tbCountryOptionsProc.Text = "";
            tbCountryOptionsProsmotr.Text = "";
            tbTokenYandex.Text = "";
            tbTokenAdprofex.Text = "";
            tbPasswordAdprofex.Text = "";
            tbLogAdprofex.Text = "";
            tbCountryOptionsUp.Text = "";
            tbCountryOptionsTime.Text = "";
            cbAdCompanyType.Text = "";
            cbCountryOptionsGroup.Text = "";
            cbProxy.IsChecked = false;
            cbWhatAdCompanys.Items.Clear();
            cbWhatCountryOptions.Items.Clear();
            cbWhatDeviceOptions.Items.Clear();
            #region Загрузка настроект потока
            cbWhatAdCompanys.Items.Clear();
            cbWhatCountryOptions.Items.Clear();
            cbWhatDeviceOptions.Items.Clear();

            lbAdPotoks.Items.Clear();
            foreach (var item in adStream)
            {
                lbAdPotoks.Items.Add(item.Name);
            }

            foreach (var ad in adCompany)
            {
                bool was = false;
                for (int i = 0; i < cbWhatAdCompanys.Items.Count; i++)
                {
                    if (cbWhatAdCompanys.Items[i].ToString() == ad.Name)
                    {
                        was = true;
                        break;

                    }
                }
                if (was) continue;
                CheckBox box = new CheckBox();

                box.Content = ad.Name;
                box.Click += CheckboxInPotoks;

                box.IsChecked = false;

                cbWhatAdCompanys.Items.Add(box);


                cbAdCompanyType.SelectedIndex = 0;

            }
            foreach (var item in OptionsCountry)
            {

                cbWhatCountryOptions.Items.Add(item.Id);




            }
            foreach (var item in OptionsDevice)
            {

                cbWhatDeviceOptions.Items.Add(item.Id);




            }
            if (adStream[0].Proxy == "True") cbProxy.IsChecked = true;
            else cbProxy.IsChecked = false;
            #endregion
            lbAdCompany.Items.Clear();
            foreach (var item in adCompany)
            {
                lbAdCompany.Items.Add(item.Id);
            }
            lbCountryOptyons.Items.Clear();
            foreach (var item in OptionsCountry)
            {
                lbCountryOptyons.Items.Add(item.Id);
            }

            tbTokenAdprofex.Text = UserOptions[0].TokenAdprofex;
            tbTokenYandex.Text = UserOptions[0].TokenYandex;
            tbPasswordAdprofex.Text = UserOptions[0].AdProfexPassword;
            tbLogAdprofex.Text = UserOptions[0].AdprofexMail;


        }


        private void lbSrlectCountryOptions(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var vs = OptionsCountry.Where(p => p.Id == Convert.ToInt64(e.AddedItems[0])).FirstOrDefault();
                tbCountryOptionsCPMV.Text = vs.CPMV.ToString();
                tbCountryOptionsDown.Text = vs.KoefDown.ToString();
                tbCountryOptionsMax.Text = vs.Max.ToString();
                tbCountryOptionsMin.Text = vs.Min.ToString();
                tbCountryOptionsName.Text = vs.Id.ToString();
                tbCountryOptionsProc.Text = vs.KoefProc.ToString();
                tbCountryOptionsProsmotr.Text = vs.Prosmotr.ToString();
                tbCountryOptionsTime.Text = vs.Time.ToString();
                tbCountryOptionsUp.Text = vs.KoefUp.ToString();
                cbCountryOptionsGroup.Text = vs.Group.ToString();
            }
            catch { }
        }
        //private void Button_Click11(object sender, RoutedEventArgs e)
        //{
        //    var vs = OptionsDevice.Where(p => p.Id == Convert.ToInt64(tbDeviceOptionsName.Text)).FirstOrDefault();
        //    if (vs != null)
        //    {
        //        OptionsDevice.Remove(vs);
        //        DeviceOptions optionsDevice = new DeviceOptions();
        //        optionsDevice.Id = Convert.ToInt64(tbDeviceOptionsName.Text);
        //        optionsDevice.KoefProc = Convert.ToInt64(tbDeviceOptionsProc.Text);
        //        optionsDevice.Prosmotr = Convert.ToInt64(tbDeviceOptionsProsmotr.Text);

        //        optionsDevice.KoefUp = Convert.ToInt64(tbDeviceOptionsUp.Text);
        //        optionsDevice.CPMV = Convert.ToDouble(tbDeviceOptionsCPMV.Text);
        //        optionsDevice.KoefDown = Convert.ToInt64(tbDeviceOptionsDown.Text);
        //        optionsDevice.Max = Convert.ToInt64(tbDeviceOptionsMax.Text);
        //        optionsDevice.Min = Convert.ToInt64(tbDeviceOptionsMin.Text);

        //        OptionsDevice.Add(optionsDevice);
        //        bool result = WorkWithBD<DeviceOptions>.Change(optionsDevice).Result;
        //        if (result)
        //        {
        //            Push push = new Push("Удачное изменение настроек устройств", "Изменение настроек");
        //            push.Show();
        //        }
        //        else
        //        {
        //            Push push = new Push("Не удачное изменение настроек устройств", "Изменение настроек");
        //            push.Show();
        //        }
        //        this.Window_Loaded(sender, e);
        //    }
        //    else
        //    {
        //        DeviceOptions optionsDevice = new DeviceOptions();
        //        optionsDevice.Id = Convert.ToInt64(tbDeviceOptionsName.Text);
        //        optionsDevice.KoefProc = Convert.ToInt64(tbDeviceOptionsProc.Text);
        //        optionsDevice.Prosmotr = Convert.ToInt64(tbDeviceOptionsProsmotr.Text);

        //        optionsDevice.KoefUp = Convert.ToInt64(tbDeviceOptionsUp.Text);
        //        optionsDevice.CPMV = Convert.ToDouble(tbDeviceOptionsCPMV.Text);
        //        optionsDevice.KoefDown = Convert.ToInt64(tbDeviceOptionsDown.Text);
        //        optionsDevice.Max = Convert.ToInt64(tbDeviceOptionsMax.Text);
        //        optionsDevice.Min = Convert.ToInt64(tbDeviceOptionsMin.Text);

        //        OptionsDevice.Add(optionsDevice);
        //        WorkWithBD<DeviceOptions>.Add(optionsDevice).Wait();
        //        this.Window_Loaded(sender, e);
        //    }
        //}

        //private void Button_Click_111(object sender, RoutedEventArgs e)
        //{
        //    var vs = OptionsDevice.Where(p => p.Id == Convert.ToInt64(tbDeviceOptionsName.Text)).FirstOrDefault();
        //    if (vs != null)
        //    {
        //        OptionsDevice.Remove(vs);
        //        WorkWithBD<DeviceOptions>.Del(vs).Wait();
        //        this.Window_Loaded(sender, e);
        //    }
        //}
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            var vs = OptionsCountry.Where(p => p.Id == Convert.ToInt64(tbCountryOptionsName.Text)).FirstOrDefault();
            if (vs != null)
            {
                OptionsCountry.Remove(vs);
                CountryOprions optionsCountry = new CountryOprions();
                optionsCountry.Id = Convert.ToInt64(tbCountryOptionsName.Text);
                optionsCountry.KoefProc = Convert.ToInt64(tbCountryOptionsProc.Text);
                optionsCountry.Prosmotr = Convert.ToInt64(tbCountryOptionsProsmotr.Text);
                optionsCountry.Group = cbCountryOptionsGroup.Text;
                optionsCountry.KoefUp = Convert.ToInt64(tbCountryOptionsUp.Text);
                optionsCountry.CPMV = Convert.ToDouble(tbCountryOptionsCPMV.Text);
                optionsCountry.KoefDown = Convert.ToInt64(tbCountryOptionsDown.Text);
                optionsCountry.Max = Convert.ToInt64(tbCountryOptionsMax.Text);
                optionsCountry.Min = Convert.ToInt64(tbCountryOptionsMin.Text);
                optionsCountry.Time = Convert.ToInt64(tbCountryOptionsTime.Text);
                OptionsCountry.Add(optionsCountry);
                bool result = WorkWithBD<CountryOprions>.Change(optionsCountry).Result;
                if (result)
                {
                    Push push = new Push("Удачное изменение настроек стран", "Изменение настроек");
                    push.Show();
                }
                else
                {
                    Push push = new Push("Не удачное изменение настроек стран", "Изменение настроек");
                    push.Show();
                }
                this.Window_Loaded(sender, e);
            }
            else
            {
                CountryOprions optionsCountry = new CountryOprions();
                optionsCountry.Id = Convert.ToInt64(tbCountryOptionsName.Text);
                optionsCountry.KoefProc = Convert.ToInt64(tbCountryOptionsProc.Text);
                optionsCountry.Prosmotr = Convert.ToInt64(tbCountryOptionsProsmotr.Text);
                optionsCountry.Group = cbCountryOptionsGroup.Text;
                optionsCountry.KoefUp = Convert.ToInt64(tbCountryOptionsUp.Text);
                optionsCountry.CPMV = Convert.ToDouble(tbCountryOptionsCPMV.Text);
                optionsCountry.KoefDown = Convert.ToInt64(tbCountryOptionsDown.Text);
                optionsCountry.Max = Convert.ToInt64(tbCountryOptionsMax.Text);
                optionsCountry.Min = Convert.ToInt64(tbCountryOptionsMin.Text);
                optionsCountry.Time = Convert.ToInt64(tbCountryOptionsTime.Text);
                OptionsCountry.Add(optionsCountry);

                bool result = WorkWithBD<CountryOprions>.Add(optionsCountry).Result;
                if (result)
                {
                    Push push = new Push("Удачное добавление настроек стран", "Изменение настроек");
                    push.Show();
                }
                else
                {
                    Push push = new Push("Не удачное добавление настроек стран", "Изменение настроек");
                    push.Show();
                }
                this.Window_Loaded(sender, e);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var vs = OptionsCountry.Where(p => p.Id == Convert.ToInt64(tbCountryOptionsName.Text)).FirstOrDefault();
            if (vs != null)
            {
                OptionsCountry.Remove(vs);

                bool result = WorkWithBD<CountryOprions>.Del(vs).Result;
                if (result)
                {
                    Push push = new Push("Удачное удаление настроек стран", "Изменение настроек");
                    push.Show();
                }
                else
                {
                    Push push = new Push("Не удачное удаление настроек стран", "Изменение настроек");
                    push.Show();
                }
                this.Window_Loaded(sender, e);
            }
        }
        private void btnSaveChangeOptionsAdCompany(object sender, RoutedEventArgs e)
        {
            try
            {
                var vs = adStream.Where(p => p.Name == tbWhatAdName.Text).FirstOrDefault();

                if (vs == null)
                {
                    AdvertisingStreams optionsCountry = new AdvertisingStreams();
                    optionsCountry.Name = tbWhatAdName.Text;
                    optionsCountry.DeviceOptionsId = Convert.ToInt64(cbWhatDeviceOptions.Text);
                    optionsCountry.CountryOptionsId = Convert.ToInt64(cbWhatCountryOptions.Text);
                    optionsCountry.AdCompanyId = cbWhatAdCompanys.Text;

                    if (cbProxy.IsChecked == true)
                    {
                        optionsCountry.Proxy = "True";
                    }
                    else
                    {
                        optionsCountry.Proxy = "False";
                    }

                    optionsCountry.Id = adStream.Max(p => p.Id) + 1;

                    adStream.Add(optionsCountry);
                    bool result = WorkWithBD<AdvertisingStreams>.Add(optionsCountry).Result;
                    if (result)
                    {
                        Push push = new Push("Удачное добавление настроек потока", "Изменение настроек");
                        push.Show();
                    }
                    else
                    {
                        Push push = new Push("Не удачное добавление настроек потока", "Изменение настроек");
                        push.Show();
                    }
                }
                else
                {
                    adStream.Remove(vs);
                    AdvertisingStreams optionsCountry = new AdvertisingStreams();
                    optionsCountry.Name = vs.Name;
                    optionsCountry.DeviceOptionsId = Convert.ToInt64(cbWhatDeviceOptions.Text);
                    optionsCountry.CountryOptionsId = Convert.ToInt64(cbWhatCountryOptions.Text);
                    optionsCountry.AdCompanyId = cbWhatAdCompanys.Text;

                    if (cbProxy.IsChecked == true)
                    {
                        optionsCountry.Proxy = "True";
                    }
                    else
                    {
                        optionsCountry.Proxy = "False";
                    }

                    optionsCountry.Id = vs.Id;

                    adStream.Add(optionsCountry);
                    bool result = WorkWithBD<AdvertisingStreams>.Change(optionsCountry).Result;
                    if (result)
                    {
                        Push push = new Push("Удачное изменение настроек потока", "Изменение настроек");
                        push.Show();
                    }
                    else
                    {
                        Push push = new Push("Не удачное изменение настроек потока", "Изменение настроек");
                        push.Show();
                    }
                }
                this.Window_Loaded(sender, e);
            }
            catch
            {

            }
        }

        private void lbAdCompany_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var vs = adCompany.Where(p => p.Id == Convert.ToInt64(e.AddedItems[0])).FirstOrDefault();
                tbAdCompanyId.Text = vs.Id.ToString();
                tbAdCompanyName.Text = vs.Name;
                cbAdCompanyType.Text = vs.Type.ToString();
            }
            catch
            {

            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var vs = adCompany.Where(p => p.Id == Convert.ToInt64(tbAdCompanyId.Text)).FirstOrDefault();
            if (vs != null)
            {
                adCompany.Remove(vs);

                bool result = WorkWithBD<AdvertisingCompany>.Del(vs).Result;
                if (result)
                {
                    Push push = new Push("Удачное удаление настроек рекламной компании", "Изменение настроек");
                    push.Show();
                }
                else
                {
                    Push push = new Push("Не удачное удаление настроек рекламной компании", "Изменение настроек");
                    push.Show();
                }
                this.Window_Loaded(sender, e);
            }
        }
        private void btnUserSaveClick(object sender, RoutedEventArgs e)
        {


            UserOptions[0].TokenAdprofex = tbTokenAdprofex.Text;
            UserOptions[0].TokenYandex = tbTokenYandex.Text;
            UserOptions[0].AdprofexMail = tbLogAdprofex.Text;
            UserOptions[0].AdProfexPassword = tbPasswordAdprofex.Text;
            bool result = WorkWithBD<UserOptions>.Change(UserOptions[0]).Result;
            if (result)
            {
                Push push = new Push("Удачное изменение настроек пользователя", "Изменение настроек");

                push.Show();
            }
            else
            {
                Push push = new Push("Не удачное изменение настроек пользователя", "Изменение настроек");

                push.Show();
            }


            this.Window_Loaded(sender, e);
        }

        private void lbAdPotoks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                #region Загрузка настроект потока
                string companis = "";
                var value = adStream.Where(p => p.Name == e.AddedItems[0].ToString()).FirstOrDefault();
                tbWhatAdName.Text = value.Name;
                foreach (CheckBox ad in cbWhatAdCompanys.Items)
                {

                    if (value.AdCompanyId.Contains(ad.Content.ToString()))
                    {
                        ad.IsChecked = true;
                        companis += ad.Content + ",";
                    }
                    else
                    {
                        ad.IsChecked = false;
                    }
                    cbWhatAdCompanys.Text = companis;


                }
                foreach (long item in cbWhatCountryOptions.Items)
                {
                    if (value.CountryOptionsId == item)
                    {
                        cbWhatCountryOptions.SelectedItem = item;
                        break;
                    }
                }
                foreach (long item in cbWhatDeviceOptions.Items)
                {
                    if (adStream[0].DeviceOptionsId == item)
                    {
                        cbWhatDeviceOptions.SelectedItem = item;

                        break;
                    }
                }
                if (adStream[0].Proxy == "True") cbProxy.IsChecked = true;
                else cbProxy.IsChecked = false;
                #endregion
            }
            catch
            {

            }

        }

        private void btnAdSaveClick(object sender, RoutedEventArgs e)
        {
            var vs = adCompany.Where(p => p.Id == Convert.ToInt64(tbAdCompanyId.Text)).FirstOrDefault();
            if (vs != null)
            {
                adCompany.Remove(vs);
                AdvertisingCompany optionsCountry = new AdvertisingCompany();
                optionsCountry.Id = Convert.ToInt64(tbAdCompanyId.Text);
                optionsCountry.Name = tbAdCompanyName.Text;
                optionsCountry.Type = cbAdCompanyType.Text;
                adCompany.Add(optionsCountry);
                bool result = WorkWithBD<AdvertisingCompany>.Change(optionsCountry).Result;
                if (result)
                {
                    Push push = new Push("Удачное изменение настроек рекламной компании", "Изменение настроек");
                    push.Show();
                }
                else
                {
                    Push push = new Push("Не удачное изменение настроек рекламной компании", "Изменение настроек");
                    push.Show();
                }
            }
            else
            {
                var value = adCompany.Where(p => p.Name == tbAdCompanyName.Text).FirstOrDefault();
                if (value != null) return;
                value = adCompany.Where(p => p.Id.ToString() == tbAdCompanyId.Text).FirstOrDefault();
                if (value != null) return;
                AdvertisingCompany optionsCountry = new AdvertisingCompany();
                optionsCountry.Id = Convert.ToInt64(tbAdCompanyId.Text);
                optionsCountry.Name = tbAdCompanyName.Text;
                optionsCountry.Type = cbAdCompanyType.Text;
                adCompany.Add(optionsCountry);

                bool result = WorkWithBD<AdvertisingCompany>.Add(optionsCountry).Result;
                if (result)
                {
                    Push push = new Push("Удачное добавление настроек рекламной компании", "Изменение настроек");
                    push.Show();
                }
                else
                {
                    Push push = new Push("Не удачное добавление настроек рекламной компании", "Изменение настроек");
                    push.Show();
                }
            }
            this.Window_Loaded(sender, e);
        }

        private void btnDelChangeOptionsAdCompany(object sender, RoutedEventArgs e)
        {
            var vs = adStream.Where(p => p.Name == (tbWhatAdName.Text)).FirstOrDefault();
            if (vs != null)
            {
                adStream.Remove(vs);

                bool result = WorkWithBD<AdvertisingStreams>.Del(vs).Result;
                if (result)
                {
                    Push push = new Push("Удачное удаление настроек потока", "Изменение настроек");
                    push.Show();
                }
                else
                {
                    Push push = new Push("Не удачное удаление настроек потока", "Изменение настроек");
                    push.Show();
                }
                this.Window_Loaded(sender, e);
            }
        }
    }
}
