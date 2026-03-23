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
            foreach (var ad in adCompany)
            {
                CheckBox box = new CheckBox();
                box.Name = ad.Name;
                box.Content = ad.Id;
                if (adStream[0].AdCompanyId.Contains(ad.Id.ToString()))
                {
                    box.IsChecked = true;
                }
                else
                {
                    box.IsChecked = false;
                }
                cbWhatAdCompanys.Items.Add(box);
                cbAdCompanyType.SelectedIndex = 0;

            }
            foreach (var item in OptionsCountry)
            {
                if (adStream[0].CountryOptionsId == item.Id)
                {
                    cbWhatCountryOptions.Items.Add(item.Id);

                    cbWhatCountryOptions.SelectedIndex = 0;
                    break;
                }
            }
            foreach (var item in OptionsDevice)
            {
                if (adStream[0].DeviceOptionsId == item.Id)
                {
                    cbWhatDeviceOptions.Items.Add(item.Id);

                    cbWhatDeviceOptions.SelectedIndex = 0;
                    break;
                }
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
                WorkWithBD<CountryOprions>.Change(optionsCountry).Wait();
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
                WorkWithBD<CountryOprions>.Add(optionsCountry).Wait();
                this.Window_Loaded(sender, e);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var vs = OptionsCountry.Where(p => p.Id == Convert.ToInt64(tbCountryOptionsName.Text)).FirstOrDefault();
            if (vs != null)
            {
                OptionsCountry.Remove(vs);
                WorkWithBD<CountryOprions>.Del(vs).Wait();
                this.Window_Loaded(sender, e);
            }
        }
        private void btnSaveChangeOptionsAdCompany(object sender, RoutedEventArgs e)
        {

            var vs = adStream[0];


            adStream.Remove(vs);
            AdvertisingStreams optionsCountry = new AdvertisingStreams();
            optionsCountry.Name = vs.Name;
            optionsCountry.DeviceOptionsId = Convert.ToInt64(cbWhatDeviceOptions.Text);
            optionsCountry.CountryOptionsId = Convert.ToInt64(cbWhatCountryOptions.Text);

            string str = "";
            for (int i = 0; i < cbWhatAdCompanys.Items.Count; i++)
            {
                
                var item = cbWhatAdCompanys.Items[i] as CheckBox;
                if (item.IsChecked == true)
                {
                    str += item.Content+",";
                }
            }
            optionsCountry.AdCompanyId = str ;

            if(cbProxy.IsChecked == true)
            {
                optionsCountry.Proxy = "True";
            }
            else
            {
                optionsCountry.Proxy = "False";
            }
        
            optionsCountry.Id =vs.Id;
           
            adStream.Add(optionsCountry);
            WorkWithBD<AdvertisingStreams>.Change(optionsCountry).Wait();
            this.Window_Loaded(sender, e);

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
                WorkWithBD<AdvertisingCompany>.Del(vs).Wait();
                this.Window_Loaded(sender, e);
            }
        }
        private void btnUserSaveClick(object sender, RoutedEventArgs e)
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
                WorkWithBD<AdvertisingCompany>.Change(optionsCountry).Wait();
            }
            else
            {
                AdvertisingCompany optionsCountry = new AdvertisingCompany();
                optionsCountry.Id = Convert.ToInt64(tbAdCompanyId.Text);
                optionsCountry.Name = tbAdCompanyName.Text;
                optionsCountry.Type = cbAdCompanyType.Text;
                adCompany.Add(optionsCountry);
                WorkWithBD<AdvertisingCompany>.Add(optionsCountry).Wait();
            }
            this.Window_Loaded(sender,e );
        }

    }
}
