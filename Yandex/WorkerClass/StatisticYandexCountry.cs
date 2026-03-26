using HarfBuzzSharp;
using Microsoft.Data.Sqlite;
using IsoNames;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Controls;
using Yandex.Model;
using Yandex.Model.ModelsBD;
using static Yandex.Model.AdpProfexApiCountry;
using static Yandex.Model.AdpProfexApiDevice;

namespace Yandex.WorkerClass
{
    public class StatisticYandexCountry
    {
        List<YandexStat?> _statCountryNow = new();
        public List<YandexStat?> StatCountryNow => _statCountryNow;
        List<YandexStat?> _sravneniecountry = new();
        public List<YandexStat?> SravnenieCountry => _sravneniecountry;
        List<YandexStat?> _sravneniedevice = new();
        public List<YandexStat?> SravnenieDevice => _sravneniedevice;
        List<YandexStat?> _statDeviceNow = new();
        public List<YandexStat?> StatDeviseNow => _statDeviceNow;
        List<YandexStat?> _statCountryDeviceLast = new();
        public List<YandexStat?> StatCountryDeviceLast => _statCountryDeviceLast;
        public static List<StatCountryDevice> StatCountryDeviceLastStatic { get; set; }
        YandexApiCountry.YandexCountry _apiContryYandex = new();
        YandexApiDevice.YandexDevice _apiDeviceYandex = new();
        List<AdpProfexApiCountry.ApiCountry> _apiCountryAdprofex = new();
        List<AdpProfexApiDevice.ApiDevice> _apiDeviceAdprofex = new();
        bool _zero;
        string _name;
        internal static UserOptions _userOptions { get; set; }
        internal static AdvertisingStreams _advertisingStreams;
        internal static List<AdvertisingCompany> _advertisingCompany;
        internal static DeviceOptions _deviceOptions;
        internal static CountryOprions _countryOptions;
        (string, ApiInfoCompainCPC) Micros1;
        (string, string) Filter { get; set; }
        public List<string> Up { get; set; } = new();
        public List<string> Down { get; set; } = new();
        public List<string> Null { get; set; } = new();
        public StatisticYandexCountry(string Name, bool Zero)
        {
            _name = Name;
            _zero = Zero;
        }
        async Task GetInfoBD()
        {
            List<AdvertisingStreams> advertisingStreams = await WorkWithBD<AdvertisingStreams>.Read(new AdvertisingStreams());
            _advertisingStreams = advertisingStreams.Where(p => p.Name == _name).FirstOrDefault();
            List<AdvertisingCompany> advertisingCompanies = await WorkWithBD<AdvertisingCompany>.Read(new AdvertisingCompany());
            _advertisingCompany = advertisingCompanies;
            List<UserOptions> user = WorkWithBD<UserOptions>.Read(new UserOptions()).Result;
            _userOptions = user[0];
            List<DeviceOptions> device = await WorkWithBD<DeviceOptions>.Read(new DeviceOptions());
            _deviceOptions = device.Where(p => p.Id == _advertisingStreams.DeviceOptionsId).FirstOrDefault();
            List<CountryOprions> country = await WorkWithBD<CountryOprions>.Read(new CountryOprions());
            _countryOptions = country.Where(p => p.Id == _advertisingStreams.CountryOptionsId).FirstOrDefault();

        }
        async Task GetStatNowAsync()
        {

            _apiContryYandex = await Api<YandexApiCountry.YandexCountry>.PostApiRespons("https://partner.yandex.ru/api/statistics2/get", Params: new (string, string)[9]
            {
                ("lang", "ru"), ("stat_type", "main") , ("period", "today") ,
                ("dimension_field", "geo|country") ,("field", "partner_wo_nds"),("field", "cpmv_partner_wo_nds"),("field", "impressions"),
                ("order_by", "[{\"field\":\"geo\",\"dir\":\"asc\"},{\"field\":\"partner_wo_nds\",\"dir\":\"asc\"}]"),("entity_field", "domain")
            }, _userOptions.TokenYandex
            );
            _apiDeviceYandex = await Api<YandexApiDevice.YandexDevice>.PostApiRespons("https://partner.yandex.ru/api/statistics2/get", Params: new (string, string)[9]
            {
                ("lang", "ru"), ("stat_type", "main") , ("period", "today") ,
                ("field", "partner_wo_nds"),("field", "cpmv_partner_wo_nds"),("field", "impressions"),
                ("order_by", "[{\"field\":\"device\",\"dir\":\"asc\"},{\"field\":\"partner_wo_nds\",\"dir\":\"asc\"}]"),("entity_field", "domain"),("entity_field", "device")
            }, _userOptions.TokenYandex
            );

        }
        async Task<bool> GetStatLastAsync()
        {
            List<StatCountryDevice> LastStat1 = await WorkWithBD<StatCountryDevice>.Read(new StatCountryDevice());
            List<YandexStat> LastStat = new List<YandexStat>();
            List<StatCountryDevice> LastStat2 = LastStat1;


            foreach (var item in LastStat1)
            {
                bool was = false;
                foreach (var stat in LastStat)
                {
                    if (item.Domen == stat.Domen)
                    {
                        if (item.Name == stat.Name)
                        {

                            DateTime a1 = Convert.ToDateTime(item.Date);
                            DateTime a2 = Convert.ToDateTime(stat.Date);
                            if (a1 > a2)
                            {
                                int i = LastStat.IndexOf(stat);
                                LastStat[i] += item;
                            }
                            was = true;
                            break;
                        }
                    }
                }
                if (LastStat.Count == 0)
                {
                    YandexStat value = new();
                    value += item;

                    LastStat.Add(value); was = true;
                }
                if (was == false)
                {
                    YandexStat value = new();
                    value += item;

                    LastStat.Add(value);

                }
            }



            _statCountryDeviceLast = LastStat;
            StatCountryDeviceLastStatic = LastStat2;
            return true;
        }
        async Task GetStatAdprofexNow()
        {
            try
            {
                string[] valuseDate = new string[] { DateTime.Today.ToString(), DateTime.Today.ToString().Replace("0:00:00", "23:59:59") };
                string[] ac = _advertisingStreams.AdCompanyId.Split(",", StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ac.Length; i++)
                {
                    CompaingFields compaingFields = PreparationForRequst.InfoForZaprosAdFox(valuseDate, ac[i], "country");//формироввание объекта для запроса
                    string obj = obj = JsonConvert.SerializeObject(compaingFields);
                    // string requsttype = $"CPC\nПолучение статы AdProfex \n Rk:{potoki.Name}";
                    ApiCountry getStat = await Api<ApiCountry>.PostApiRespons("https://adv-api.adprofex.com/api/filters/cabinet-statistics?page=1&perPage=100000&sort=-dsp_flow", obj, _userOptions.TokenAdprofex);

                    _apiCountryAdprofex.Add(getStat);

                }
            }
            catch { }
            try
            {
                string[] valuseDate = new string[] { DateTime.Today.ToString(), DateTime.Today.ToString().Replace("0:00:00", "23:59:59") };
                string[] ac = _advertisingStreams.AdCompanyId.Split(",", StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < ac.Length; i++)
                {
                    CompaingFields compaingFields = PreparationForRequst.InfoForZaprosAdFox(valuseDate, ac[i], "os");//формироввание объекта для запроса
                    string obj = obj = JsonConvert.SerializeObject(compaingFields);
                    // string requsttype = $"CPC\nПолучение статы AdProfex \n Rk:{potoki.Name}";
                    ApiDevice getStat = await Api<ApiDevice>.PostApiRespons("https://adv-api.adprofex.com/api/filters/cabinet-statistics?page=1&perPage=100000&sort=-dsp_flow", obj, _userOptions.TokenAdprofex);

                    _apiDeviceAdprofex.Add(getStat);

                }
            }
            catch { }
        }

        async Task Sravnenie()
        {
            for (int h = 1; h < _apiCountryAdprofex.Count; h++)
            {

                for (int i = 0; i < _apiCountryAdprofex[0].data.Count; i++)
                {
                    var st = _apiCountryAdprofex[h].data.Where(p => p.country == _apiCountryAdprofex[0].data[i].country).FirstOrDefault();
                    if (st != null)
                    {
                        _apiCountryAdprofex[0].data[i] = _apiCountryAdprofex[0].data[i] + st;
                        _apiCountryAdprofex[h].data.Remove(st);
                    }
                }
            }
            for (int h = 0; h < _apiCountryAdprofex.Count; h++)
            {
                for (int j = 0; j < _apiContryYandex.data.points.Count; j++)
                {
                    bool was = true;

                    for (int i = 0; i < _apiCountryAdprofex[h].data.Count - 1; i++)
                    {
                        try
                        {


                            string country = Translate(_apiCountryAdprofex[h].data[i].country);
                            _apiContryYandex.data.points[j].dimensions.geo = _apiContryYandex.data.points[j].dimensions.geo.Replace("ё", "е");
                            if (country == "Чешская республика") country = "Чехия";
                            if (_apiContryYandex.data.points[j].dimensions.geo == "Киргизия") _apiContryYandex.data.points[j].dimensions.geo = "Кыргызстан";
                            if (_apiContryYandex.data.points[j].dimensions.geo == country)
                            {
                                if (_statCountryNow.FirstOrDefault(p => p.Name == _apiContryYandex.data.points[j].dimensions.geo) == null)
                                {
                                    if (_statCountryNow.FirstOrDefault(p => p.Name == country) == null)
                                    {
                                        YandexStat statCountryDevice = new();
                                        statCountryDevice.Domen = _apiContryYandex.data.points[j].dimensions.domain;
                                        statCountryDevice.Name = _apiContryYandex.data.points[j].dimensions.geo;
                                        statCountryDevice.CPMV = _apiContryYandex.data.points[j].measures[0].cpmv_partner_wo_nds;
                                        statCountryDevice.Dohod = _apiContryYandex.data.points[j].measures[0].partner_wo_nds;
                                        statCountryDevice.Prosmotr = _apiContryYandex.data.points[j].measures[0].impressions;
                                        statCountryDevice.CPC = Convert.ToDouble(_apiCountryAdprofex[h].data[i].click_price_dsp);
                                        statCountryDevice.Click = Convert.ToInt64(_apiCountryAdprofex[h].data[i].click_count);
                                        statCountryDevice.Date = DateTime.Now;
                                        statCountryDevice.Rr = _apiContryYandex.data.points[j].measures[0].partner_wo_nds / _apiCountryAdprofex[h].data[i].dsp_flow;
                                        statCountryDevice.Rashod = _apiCountryAdprofex[h].data[i].dsp_flow;
                                        statCountryDevice.DohodClear = _apiContryYandex.data.points[j].measures[0].partner_wo_nds - _apiCountryAdprofex[h].data[i].dsp_flow;
                                        string id = ChangeCountry(statCountryDevice.Name);
                                        statCountryDevice.Kof = Micros1.Item2.campaign_micro_bidding._3.Where(p => p.id == id).First().coeff;
                                        _statCountryNow.Add(statCountryDevice);
                                        was = false;
                                        break;
                                    }
                                }
                                else
                                {

                                    var stat = _statCountryNow.FirstOrDefault(p => p.Name == _apiContryYandex.data.points[j].dimensions.geo);
                                    stat.CPC = Math.Round(stat.CPC + Convert.ToDouble(_apiCountryAdprofex[h].data[i].click_price_dsp) / 2, 2);
                                    stat.Rashod += _apiCountryAdprofex[h].data[i].dsp_flow;
                                    stat.DohodClear = stat.Dohod - stat.Rashod;

                                    was = false;

                                    break;

                                }

                            }
                            //else
                            //{

                            //    var stat = _statCountryNow.FirstOrDefault(p => p.Name == _apiContryYandex.data.points[j].dimensions.geo);                                
                            //    stat.CPC = Math.Round(stat.CPC + Convert.ToDouble(_apiCountryAdprofex[h].data[i].click_price_dsp) / 2, 2);
                            //    stat.Rashod += _apiCountryAdprofex[h].data[i].dsp_flow;
                            //    stat.DohodClear = stat.Dohod - stat.Rashod;

                            //    was = false;



                            //}
                        }
                        catch { continue; }
                    }
                    if (was)
                    {
                        if (_statCountryNow.FirstOrDefault(p => p.Name == _apiContryYandex.data.points[j].dimensions.geo) == null)
                        {
                            YandexStat statCountryDevice = new();
                            statCountryDevice.Domen = _apiContryYandex.data.points[j].dimensions.domain;
                            statCountryDevice.Name = _apiContryYandex.data.points[j].dimensions.geo;
                            statCountryDevice.CPMV = _apiContryYandex.data.points[j].measures[0].cpmv_partner_wo_nds;
                            statCountryDevice.Dohod = _apiContryYandex.data.points[j].measures[0].partner_wo_nds;
                            statCountryDevice.Prosmotr = _apiContryYandex.data.points[j].measures[0].impressions;
                            statCountryDevice.Click = 0;
                            statCountryDevice.CPC = 0;
                            statCountryDevice.Click = 0;
                            statCountryDevice.Date = DateTime.Now;
                            statCountryDevice.Rr = _apiContryYandex.data.points[j].measures[0].partner_wo_nds;
                            statCountryDevice.Rashod = 0;
                            statCountryDevice.DohodClear = _apiContryYandex.data.points[j].measures[0].partner_wo_nds;
                            string id = ChangeCountry(statCountryDevice.Name);

                            try
                            {
                                statCountryDevice.Kof = Micros1.Item2.campaign_micro_bidding._3.Where(p => p.id == id).First().coeff;
                                _statCountryNow.Add(statCountryDevice);
                            }
                            catch
                            {
                                statCountryDevice.Kof = 0;
                                _statCountryNow.Add(statCountryDevice);

                            }
                        }
                    }
                }
            }
            for (int i = 0; i < this._apiDeviceAdprofex.Count; i++)
            {
                for (int j = 0; j < this._apiDeviceAdprofex[i].data.Count - 1; j++)
                {
                    if (this._apiDeviceAdprofex[i].data[j].os == "Android" || this._apiDeviceAdprofex[i].data[j].os == "iOS")
                    {
                        this._apiDeviceAdprofex[i].data[j].os = "Моб.";
                    }
                    else
                    {
                        this._apiDeviceAdprofex[i].data[j].os = "ПК";
                    }
                }
                int indexPk = 0;
                int indexMob = 0;


                AdpProfexApiDevice.Datum mob = this._apiDeviceAdprofex[i].data.Find(p => p.os == "Моб.");
                AdpProfexApiDevice.Datum pk = this._apiDeviceAdprofex[i].data.Find(p => p.os == "ПК");
                if (mob == null) mob = new();
                if (pk == null) pk = new();
                try
                {
                    for (int h = 0; h < this._apiDeviceAdprofex[i].data.Count; h++)
                    {
                        if (this._apiDeviceAdprofex[i].data[h] == mob)
                        {
                            this._apiDeviceAdprofex[i].data.Remove(this._apiDeviceAdprofex[i].data[h]);
                            h--;
                        }
                        else if (this._apiDeviceAdprofex[i].data[h] == pk)
                        {
                            this._apiDeviceAdprofex[i].data.Remove(this._apiDeviceAdprofex[i].data[h]);
                            h--;
                        }
                    }

                    for (int j = 0; j < this._apiDeviceAdprofex[i].data.Count; j++)
                    {
                        if (this._apiDeviceAdprofex[i].data[j].os == "ПК")
                        {
                            if (this._apiDeviceAdprofex[i].data[j].os == pk.os)
                            {
                                indexPk++;
                                try
                                {
                                    pk.buy_count = (int.Parse(pk.buy_count) + int.Parse(this._apiDeviceAdprofex[i].data[j].buy_count)).ToString();
                                    pk.click_count = pk.click_count + this._apiDeviceAdprofex[i].data[j].click_count;
                                    pk.click_price_dsp = pk.click_price_dsp + this._apiDeviceAdprofex[i].data[j].click_price_dsp;
                                    pk.ctr = pk.ctr + this._apiDeviceAdprofex[i].data[j].ctr;
                                }
                                catch { }
                                pk.dsp_flow = pk.dsp_flow + this._apiDeviceAdprofex[i].data[j].dsp_flow;

                                this._apiDeviceAdprofex[i].data.Remove(this._apiDeviceAdprofex[i].data[j]);

                                j--;
                            }
                        }
                        else
                        {
                            if (this._apiDeviceAdprofex[i].data[j].os == mob.os)
                            {
                                indexMob++;
                                try
                                {
                                    mob.buy_count = (int.Parse(mob.buy_count) + int.Parse(this._apiDeviceAdprofex[i].data[j].buy_count)).ToString();
                                    mob.click_count = mob.click_count + this._apiDeviceAdprofex[i].data[j].click_count;
                                    mob.click_price_dsp = mob.click_price_dsp + this._apiDeviceAdprofex[i].data[j].click_price_dsp;
                                    mob.ctr = mob.ctr + this._apiDeviceAdprofex[i].data[j].ctr;
                                }
                                catch { }
                                mob.dsp_flow = mob.dsp_flow + this._apiDeviceAdprofex[i].data[j].dsp_flow;
                                this._apiDeviceAdprofex[i].data.Remove(this._apiDeviceAdprofex[i].data[j]);
                                j--;

                            }
                        }
                    }
                    if (indexMob > 0)
                    {
                        mob.click_price_dsp = Math.Round(mob.click_price_dsp / indexMob, 3);
                        mob.ctr = Math.Round(mob.ctr / indexMob, 3);
                    }
                    mob.dsp_flow = Math.Round(mob.dsp_flow, 3);
                    if (indexPk > 0)
                    {
                        pk.click_price_dsp = Math.Round(pk.click_price_dsp / indexPk, 3);
                        pk.ctr = Math.Round(pk.ctr / indexMob, 3);
                    }
                    pk.dsp_flow = Math.Round(pk.dsp_flow, 3);
                    _apiDeviceAdprofex[i].data.Clear();
                    _apiDeviceAdprofex[i].data.Add(pk);
                    _apiDeviceAdprofex[i].data.Add(mob);
                }
                catch { }
            }
            for (int h = 1; h < _apiDeviceAdprofex.Count; h++)
            {

                for (int i = 0; i < _apiDeviceAdprofex[0].data.Count; i++)
                {
                    var st = _apiDeviceAdprofex[h].data.Where(p => p.os == _apiDeviceAdprofex[0].data[i].os).FirstOrDefault();
                    if (st != null)
                    {
                        _apiDeviceAdprofex[0].data[i] = _apiDeviceAdprofex[0].data[i] + st;
                        _apiDeviceAdprofex[h].data.Remove(st);
                    }
                }
            }
            for (int j = 0; j < _apiDeviceYandex.data.points.Count; j++)
            {
                for (int h = 0; h < _apiDeviceAdprofex.Count; h++)
                {
                    bool was = true;
                    for (int i = 0; i < _apiDeviceAdprofex[h].data.Count; i++)
                    {
                        string country = TranslateOs(_apiDeviceAdprofex[h].data[i].os);
                        if (_apiDeviceYandex.data.points[j].dimensions.device == country)
                        {
                            if (_statDeviceNow.FirstOrDefault(p => p.Name == _apiDeviceYandex.data.points[j].dimensions.device) == null)
                            {
                                YandexStat statCountryDevice = new();
                                statCountryDevice.Domen = _apiDeviceYandex.data.points[j].dimensions.domain;
                                statCountryDevice.Name = _apiDeviceYandex.data.points[j].dimensions.device;
                                statCountryDevice.CPMV = _apiDeviceYandex.data.points[j].measures[0].cpmv_partner_wo_nds;
                                statCountryDevice.Dohod = _apiDeviceYandex.data.points[j].measures[0].partner_wo_nds;
                                statCountryDevice.Prosmotr = _apiDeviceYandex.data.points[j].measures[0].impressions;
                                statCountryDevice.Click = Convert.ToInt64(_apiDeviceAdprofex[h].data[i].buy_count);
                                statCountryDevice.CPC = Convert.ToDouble(_apiDeviceAdprofex[h].data[i].click_price_dsp);
                                statCountryDevice.Click = Convert.ToInt64(_apiDeviceAdprofex[h].data[i].click_count);
                                statCountryDevice.Date = DateTime.Now;
                                statCountryDevice.Rr = _apiDeviceYandex.data.points[j].measures[0].partner_wo_nds / _apiDeviceAdprofex[h].data[i].dsp_flow;
                                statCountryDevice.Rashod = _apiDeviceAdprofex[h].data[i].dsp_flow;
                                statCountryDevice.DohodClear = _apiDeviceYandex.data.points[j].measures[0].partner_wo_nds - _apiDeviceAdprofex[h].data[i].dsp_flow;
                                if (statCountryDevice.Name == "Мобильный телефон")
                                {
                                    statCountryDevice.Kof = this.Micros1.Item2.campaign_micro_bidding._1[1].coeff;
                                }
                                else if (statCountryDevice.Name == "Компьютер")
                                {
                                    statCountryDevice.Kof = this.Micros1.Item2.campaign_micro_bidding._1[0].coeff;

                                }
                                _statDeviceNow.Add(statCountryDevice);
                                was = false;
                                break;
                            }
                        }
                    }
                    if (was)
                    {
                        if (_statDeviceNow.FirstOrDefault(p => p.Name == _apiDeviceYandex.data.points[j].dimensions.device) == null)
                        {
                            YandexStat statCountryDevice = new();
                            statCountryDevice.Domen = _apiDeviceYandex.data.points[j].dimensions.domain;
                            statCountryDevice.Name = _apiDeviceYandex.data.points[j].dimensions.device;
                            statCountryDevice.CPMV = _apiDeviceYandex.data.points[j].measures[0].cpmv_partner_wo_nds;
                            statCountryDevice.Dohod = _apiDeviceYandex.data.points[j].measures[0].partner_wo_nds;
                            statCountryDevice.Prosmotr = _apiDeviceYandex.data.points[j].measures[0].impressions;
                            statCountryDevice.Click = 0;
                            statCountryDevice.CPC = 0;
                            statCountryDevice.Click = 0;
                            statCountryDevice.Date = DateTime.Now;
                            statCountryDevice.Rr = _apiDeviceYandex.data.points[j].measures[0].partner_wo_nds;
                            statCountryDevice.Rashod = 0;
                            statCountryDevice.DohodClear = _apiDeviceYandex.data.points[j].measures[0].partner_wo_nds;
                            try
                            {
                                if (statCountryDevice.Name == "Мобильный телефон")
                                {
                                    statCountryDevice.Kof = this.Micros1.Item2.campaign_micro_bidding._1[1].coeff;
                                }
                                else if (statCountryDevice.Name == "Компьютер")
                                {
                                    statCountryDevice.Kof = this.Micros1.Item2.campaign_micro_bidding._1[0].coeff;

                                }
                                _statDeviceNow.Add(statCountryDevice);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < _statCountryNow.Count; i++)
            {
                for (int j = 0; j < _statCountryDeviceLast.Count; j++)
                {
                    if (_statCountryNow[i].Name == _statCountryDeviceLast[j].Name)
                    {
                        if (_statCountryNow[i].Domen == _statCountryDeviceLast[j].Domen)
                        {
                            var vs = _statCountryNow[i] + _statCountryDeviceLast[j];
                            _sravneniecountry.Add(vs);
                            if (vs.DohodClear > 0)
                            {
                                if (_statCountryNow[i].CPMV > _countryOptions.CPMV)
                                {
                                    Up.Add(_statCountryNow[i].Name);
                                    _statCountryNow[i].Kof += _countryOptions.KoefUp;
                                }
                            }
                            if (vs.DohodClear < 0)
                            {
                                if (vs.Click > 0)
                                {
                                    Down.Add(_statCountryNow[i].Name);
                                    _statCountryNow[i].Kof -= _countryOptions.KoefDown;
                                }

                            }
                            if (vs.DohodClear == 0 && _zero)
                            {
                                if (this._statCountryNow[i].Kof > _countryOptions.Min) //проверяем коэф если больше минимального для нулей проходим дальше
                                {
                                    if (vs.Prosmotr <= _countryOptions.Prosmotr)//проверяем показы если было меньше показов чем в настройках проходим дальше
                                    {
                                        if ((this._statCountryNow[i].Kof + _countryOptions.KoefUp) < _countryOptions.Max)// если коэф + коэф увеличения меньше максимального для нулей проходим дальше
                                        {
                                            this.Null.Add(vs.Name);//добавляем в нули 
                                            this._statCountryNow[i].Kof += _countryOptions.KoefUp;
                                        }
                                    }
                                }
                            }


                        }
                    }
                }
            }
            for (int i = 0; i < _statDeviceNow.Count; i++)
            {
                for (int j = 0; j < _statCountryDeviceLast.Count; j++)
                {
                    if (_statDeviceNow[i].Name == _statCountryDeviceLast[j].Name)
                    {
                        if (_statDeviceNow[i].Domen == _statCountryDeviceLast[j].Domen)
                        {
                            var vs = _statDeviceNow[i] + _statCountryDeviceLast[j];
                            _sravneniedevice.Add(vs);
                            if (vs.DohodClear > 0)
                            {
                                if (_statDeviceNow[i].CPMV > _deviceOptions.CPMV)
                                {
                                    Up.Add(_statDeviceNow[i].Name);
                                    _statDeviceNow[i].Kof += _deviceOptions.KoefUp;
                                }
                            }
                            if (vs.DohodClear < 0)
                            {
                                if (vs.Click > 0)
                                {
                                    Down.Add(_statDeviceNow[i].Name);
                                    _statDeviceNow[i].Kof -= _deviceOptions.KoefDown;
                                }

                            }
                            if (vs.DohodClear == 0 && _zero)
                            {
                                if (this._statDeviceNow[i].Kof > _deviceOptions.Min) //проверяем коэф если больше минимального для нулей проходим дальше
                                {
                                    if (vs.Prosmotr <= _deviceOptions.Prosmotr)//проверяем показы если было меньше показов чем в настройках проходим дальше
                                    {
                                        if ((this._statDeviceNow[i].Kof + _deviceOptions.KoefUp) < _deviceOptions.Max)// если коэф + коэф увеличения меньше максимального для нулей проходим дальше
                                        {
                                            this.Null.Add(vs.Name);//добавляем в нули 
                                            this._statDeviceNow[i].Kof += _deviceOptions.KoefUp;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            await Save();
        }
        async Task GetInfo()
        {
            string filter = "";//не всегда проходит токен
            string s;
            (string, string, ApiInfoCompainCPC) rez = ("", "", null);

            List<MicroCountry> microCountries = new List<MicroCountry>();


            string[] ss = _advertisingStreams.AdCompanyId.Split(",", StringSplitOptions.RemoveEmptyEntries);
            for (int h = 0; h < ss.Length; h++)
            {


                s = await Api<string>.GetApiResponsString($"https://adv-api.adprofex.com/api/campaign/{ss[h]}", _userOptions.TokenAdprofex);
                //WorkClass.WriteLog(date, "Получение инфы об рк", s.Item2);
                try
                {


                    int indexStart = s.IndexOf("\"filter\":{\"values\":");
                    int indexEnd = s.IndexOf("\",\"is_white_list");
                    string blak = s.Substring(indexStart + 20, indexEnd - indexStart - 20);
                    if (blak == "")

                    {
                        blak = "1-1";
                    }
                    filter = "\"filter\": {\"values\":" + $"\"{blak}\"" + ",\"is_white_list\": false},";
                    indexStart = s.IndexOf("\"campaign_micro_bidding\":");
                    indexEnd = s.IndexOf(",\"schedule\":");
                    string micro = s.Substring(indexStart, indexEnd - indexStart);
                    micro = micro.Insert(0, "{");
                    micro = micro.Insert(micro.Length, "}");
                    ApiInfoCompainCPC myDeserializedClass = JsonConvert.DeserializeObject<ApiInfoCompainCPC>(micro);

                    rez = (_advertisingCompany.Where(p => p.Name.Contains(_advertisingStreams.Name)).First().Id.ToString(), filter, myDeserializedClass);
                    for (int i = 0; i < rez.Item3.campaign_micro_bidding._3.Count - 1; i++)
                    {
                        if (rez.Item3.campaign_micro_bidding._3[i].id == rez.Item3.campaign_micro_bidding._3[i + 1].id)
                        {
                            rez.Item3.campaign_micro_bidding._3.Remove(rez.Item3.campaign_micro_bidding._3[i]);
                            i--;
                        }

                    }
                    for (int i = 0; i < rez.Item3.campaign_micro_bidding._1.Count - 1; i++)
                    {
                        if (rez.Item3.campaign_micro_bidding._1[i].id == rez.Item3.campaign_micro_bidding._1[i + 1].id)
                        {
                            rez.Item3.campaign_micro_bidding._1.Remove(rez.Item3.campaign_micro_bidding._1[i]);
                            i--;
                        }
                    }
                    for (int i = 0; i < rez.Item3.campaign_micro_bidding._2.Count - 1; i++)
                    {
                        if (rez.Item3.campaign_micro_bidding._2[i].id == rez.Item3.campaign_micro_bidding._2[i + 1].id)
                        {
                            rez.Item3.campaign_micro_bidding._2.Remove(rez.Item3.campaign_micro_bidding._2[i]);
                            i--;
                        }
                    }
                    this.Micros1 = (rez.Item1, rez.Item3);


                    this.Filter = (rez.Item1, rez.Item2);
                }
                catch { }
            }
        }



        public async Task Start()
        {
            await GetInfoBD();
            await GetStatNowAsync();
            await GetStatAdprofexNow();
            await GetInfo();
            bool can = await GetStatLastAsync();
            while (can != true)
            {
                await Task.Delay(1000);
            }
            await Sravnenie();
            await ChangeKoefAsync();
        }
        async Task Save()
        {
            for (int i = 0; i < _statCountryNow.Count; i++)
            {

                await WorkWithBD<StatCountryDevice>.Add(new StatCountryDevice() + _statCountryNow[i]);
                StatCountryDeviceLastStatic.Add(new StatCountryDevice() + _statCountryNow[i]);
            }
            for (int i = 0; i < _statDeviceNow.Count; i++)
            {
                await WorkWithBD<StatCountryDevice>.Add(new StatCountryDevice() + _statDeviceNow[i]);
                StatCountryDeviceLastStatic.Add(new StatCountryDevice() + _statDeviceNow[i]);
            }

        }
        async Task ChangeKoefAsync()
        {
            List<string> _Up = new();
            
            for (int i = 0; i < Up.Count; i++)
            {
                if (Up[i] == "Мобильный телефон")
                {
                    this.Micros1.Item2.campaign_micro_bidding._1[1].coeff += (int)_deviceOptions.KoefUp;
                    continue;
                }
                else if (Up[i] == "Компьютер")
                {
                    this.Micros1.Item2.campaign_micro_bidding._1[0].coeff += (int)_deviceOptions.KoefUp;
                    continue;

                }
                
                string vs = ChangeCountry(Up[i]);

                _Up.Add(vs);
            }
            List<string> _Down = new();
            for (int i = 0; i < Down.Count; i++)
            {
                if (Up[i] == "Мобильный телефон")
                {
                    this.Micros1.Item2.campaign_micro_bidding._1[1].coeff -= (int)_deviceOptions.KoefDown;
                    continue;
                }
                else if (Up[i] == "Компьютер")
                {
                    this.Micros1.Item2.campaign_micro_bidding._1[0].coeff -= (int)_deviceOptions.KoefDown;
                    continue;

                }
                string vs = ChangeCountry(Down[i]);

                _Down.Add(vs);
            }
            List<string> _Nuls = new();
            for (int i = 0; i < Null.Count; i++)
            {
                if (Up[i] == "Мобильный телефон")
                {
                    this.Micros1.Item2.campaign_micro_bidding._1[1].coeff += (int)_deviceOptions.KoefUp;
                    continue;
                }
                else if (Up[i] == "Компьютер")
                {
                    this.Micros1.Item2.campaign_micro_bidding._1[0].coeff += (int)_deviceOptions.KoefUp;
                    continue;

                }
                string vs = ChangeCountry(Null[i]);

                _Nuls.Add(vs);
            }
            for (int i = 0; i < _Up.Count; i++)
            {
                for (int j = 0; j < this.Micros1.Item2.campaign_micro_bidding._3.Count; j++)
                {

                    if ((this.Micros1.Item2.campaign_micro_bidding._3[j].id == Convert.ToString(_Up[i])) && (Convert.ToInt32(this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) > 1))
                    {
                        this.Micros1.Item2.campaign_micro_bidding._3[j].coeff = (int)(Convert.ToInt32(this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) + (_countryOptions.KoefUp));
                    }

                }
            }
            for (int i = 0; i < _Nuls.Count; i++)
            {
                for (int j = 0; j < this.Micros1.Item2.campaign_micro_bidding._3.Count; j++)
                {

                    if (this.Micros1.Item2.campaign_micro_bidding._3[j].id == Convert.ToString(_Nuls[i]))
                    {
                        if ((this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) >= (_countryOptions.Min) && (this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) < (_countryOptions.Max))
                        {
                            this.Micros1.Item2.campaign_micro_bidding._3[j].coeff = (Convert.ToInt32(this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) + (int)(_countryOptions.KoefUp));
                        }
                    }

                }
            }
            for (int i = 0; i < _Down.Count; i++)
            {
                for (int j = 0; j < this.Micros1.Item2.campaign_micro_bidding._3.Count; j++)
                {

                    if (this.Micros1.Item2.campaign_micro_bidding._3[j].id == Convert.ToString(_Down[i]))
                    {
                        if ((this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) - (_countryOptions.KoefDown) > 1)
                        {
                            this.Micros1.Item2.campaign_micro_bidding._3[j].coeff = (Convert.ToInt32(this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) - (int)(_countryOptions.KoefDown));
                        }
                        else if ((this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) > 1)
                        {

                            this.Micros1.Item2.campaign_micro_bidding._3[j].coeff = 2;
                        }
                    }

                }
            }

            string countrys = "";
            string filter = this.Filter.Item2;
            for (int i = 0; i < this.Micros1.Item2.campaign_micro_bidding._3.Count; i++)
            {
                if (this.Micros1.Item2.campaign_micro_bidding._3[i].coeff > 0)
                {
                    countrys += this.Micros1.Item2.campaign_micro_bidding._3[i].id + ",";
                }
            }
            countrys = countrys.Remove(countrys.Length - 1);
            string micro = "{\"country\":" +
                                $"[{countrys}],";

            micro += filter;
            micro += "\"micro_bidding\": {\r\n\"3\":\r\n[";

            micro += "{\"id\":" + this.Micros1.Item2.campaign_micro_bidding._3[0].id + ",\"coeff\":" + this.Micros1.Item2.campaign_micro_bidding._3[0].coeff + "}";
            for (int i = 1; i < this.Micros1.Item2.campaign_micro_bidding._3.Count; i++)
            {
                micro += ",{\"id\":" + this.Micros1.Item2.campaign_micro_bidding._3[i].id + ",\"coeff\":" + this.Micros1.Item2.campaign_micro_bidding._3[i].coeff + "}";
            }
            if (this.Micros1.Item2.campaign_micro_bidding._2 == null)
            {
                micro += "],\r\n\"2\":\r\n[{\"id\":0,\"coeff\":100},{\"id\":1,\"coeff\":100},{\"id\":2,\"coeff\":100},{\"id\":3,\"coeff\":100},{\"id\":4,\"coeff\":100},{\"id\":5,\"coeff\":100},{\"id\":6,\"coeff\":100},{\"id\":7,\"coeff\":100},{\"id\":8,\"coeff\":100},{\"id\":9,\"coeff\":100},{\"id\":10,\"coeff\":100},{\"id\":11,\"coeff\":100},{\"id\":12,\"coeff\":100},{\"id\":13,\"coeff\":100},{\"id\":14,\"coeff\":100},{\"id\":15,\"coeff\":100},{\"id\":16,\"coeff\":100},{\"id\":17,\"coeff\":100},{\"id\":18,\"coeff\":100},{\"id\":19,\"coeff\":100},{\"id\":20,\"coeff\":100},{\"id\":21,\"coeff\":100},{\"id\":22,\"coeff\":100},{\"id\":23,\"coeff\":100}";
            }
            else
            {

                micro += "],\r\n\"2\":\r\n[";
                micro += "{\"id\":" + this.Micros1.Item2.campaign_micro_bidding._2[0].id + ",\"coeff\":" + this.Micros1.Item2.campaign_micro_bidding._2[0].coeff + "}";
                for (int i = 1; i < this.Micros1.Item2.campaign_micro_bidding._2.Count; i++)
                {
                    micro += ",{\"id\":" + this.Micros1.Item2.campaign_micro_bidding._2[i].id + ",\"coeff\":" + this.Micros1.Item2.campaign_micro_bidding._2[i].coeff + "}";
                }
            }
            micro += "],\r\n\"1\":\r\n[{\"id\":" + "1" + ",\"coeff\":" + this.Micros1.Item2.campaign_micro_bidding._1[0].coeff + "}";

            micro += ",{\"id\":" + "3" + ",\"coeff\":" + this.Micros1.Item2.campaign_micro_bidding._1[1].coeff + "}]}";

            string group = "";
            switch (_countryOptions.Group)
            {
                case "Все": group = "[2,3,1]"; break;
                case "Премиум": group = "[3]"; break;
                case "Медиум": group = "[2]"; break;
                case "Бомжи": group = "[1]"; break;
                case "Без бомжей": group = "[2,3]"; break;
                case "Без Медиума": group = "[3,1]"; break;
                case "Без премиума":

                    group = "[2,1]"; break;
            }
            string[] ss = _advertisingStreams.AdCompanyId.Split(",", StringSplitOptions.RemoveEmptyEntries);
            for (int h = 0; h < ss.Length; h++)
            {
                string micro1 = micro;
                if (_advertisingCompany.Where(p => p.Id.ToString() == ss[h]).First().Type == "Push")
                {
                   
                    micro1 += $",\"active_site_groups\":{group},\"min_subscription_days\":0,\"max_subscription_days\":9999";
                }
                if (_advertisingCompany.Where(p => p.Id.ToString() == ss[h]).First().Type == "Vitrina")
                {
                  
                    micro1 += $",\"active_site_groups\":{group}";
                }
                if (_advertisingStreams.Proxy == "True")
                {
                    micro1 += ",\"exclude_proxy_ips\":true";
                }
                else
                {
                    micro1 += ",\"exclude_proxy_ips\":false";
                }
                micro1 += "}";

                Api<bool>.PutApiRespons($"https://adv-api.adprofex.com/api/campaign/{ss[h]}", micro1, _userOptions.TokenAdprofex);
            }

        }
        public async void ChangeKoefRukami(YandexStat changed)
        {
            if (changed.Name == "Мобильный телефон")
            {
                this.Micros1.Item2.campaign_micro_bidding._1[1].coeff = (int)changed.Kof;
            }
            else if (changed.Name == "Компьютер")
            {
                this.Micros1.Item2.campaign_micro_bidding._1[0].coeff = (int)changed.Kof;

            }
            else
            {
                string item = ChangeCountry(changed.Name);

                changed.Name = item;
                for (int i = 0; i < this.Micros1.Item2.campaign_micro_bidding._3.Count; i++)
                {

                    if (this.Micros1.Item2.campaign_micro_bidding._3[i].id == changed.Name)
                    {
                        this.Micros1.Item2.campaign_micro_bidding._3[i].coeff = (int)changed.Kof;
                        break;

                    }

                }
            }
            string countrys = "";
            string filter = this.Filter.Item2;
            for (int i = 0; i < this.Micros1.Item2.campaign_micro_bidding._3.Count; i++)
            {
                if (this.Micros1.Item2.campaign_micro_bidding._3[i].coeff > 0)
                {
                    countrys += this.Micros1.Item2.campaign_micro_bidding._3[i].id + ",";
                }
            }
            countrys = countrys.Remove(countrys.Length - 1);
            string micro = "{\"country\":" +
                                $"[{countrys}],";
            micro += filter;
            micro += "\"micro_bidding\": {\r\n\"3\":\r\n[";

            micro += "{\"id\":" + this.Micros1.Item2.campaign_micro_bidding._3[0].id + ",\"coeff\":" + this.Micros1.Item2.campaign_micro_bidding._3[0].coeff + "}";
            for (int i = 1; i < this.Micros1.Item2.campaign_micro_bidding._3.Count; i++)
            {
                micro += ",{\"id\":" + this.Micros1.Item2.campaign_micro_bidding._3[i].id + ",\"coeff\":" + this.Micros1.Item2.campaign_micro_bidding._3[i].coeff + "}";
            }
            if (this.Micros1.Item2.campaign_micro_bidding._2 == null)
            {
                micro += "],\r\n\"2\":\r\n[{\"id\":0,\"coeff\":100},{\"id\":1,\"coeff\":100},{\"id\":2,\"coeff\":100},{\"id\":3,\"coeff\":100},{\"id\":4,\"coeff\":100},{\"id\":5,\"coeff\":100},{\"id\":6,\"coeff\":100},{\"id\":7,\"coeff\":100},{\"id\":8,\"coeff\":100},{\"id\":9,\"coeff\":100},{\"id\":10,\"coeff\":100},{\"id\":11,\"coeff\":100},{\"id\":12,\"coeff\":100},{\"id\":13,\"coeff\":100},{\"id\":14,\"coeff\":100},{\"id\":15,\"coeff\":100},{\"id\":16,\"coeff\":100},{\"id\":17,\"coeff\":100},{\"id\":18,\"coeff\":100},{\"id\":19,\"coeff\":100},{\"id\":20,\"coeff\":100},{\"id\":21,\"coeff\":100},{\"id\":22,\"coeff\":100},{\"id\":23,\"coeff\":100}";
            }
            else
            {

                micro += "],\r\n\"2\":\r\n[";
                micro += "{\"id\":" + this.Micros1.Item2.campaign_micro_bidding._2[0].id + ",\"coeff\":" + this.Micros1.Item2.campaign_micro_bidding._2[0].coeff + "}";
                for (int i = 1; i < this.Micros1.Item2.campaign_micro_bidding._2.Count; i++)
                {
                    micro += ",{\"id\":" + this.Micros1.Item2.campaign_micro_bidding._2[i].id + ",\"coeff\":" + this.Micros1.Item2.campaign_micro_bidding._2[i].coeff + "}";
                }
            }
            micro += "],\r\n\"1\":\r\n[{\"id\":" + "1" + ",\"coeff\":" + this.Micros1.Item2.campaign_micro_bidding._1[0].coeff + "}";

            micro += ",{\"id\":" + "3" + ",\"coeff\":" + this.Micros1.Item2.campaign_micro_bidding._1[1].coeff + "}]}";
            string group = "";
            switch (_countryOptions.Group)
            {
                case "Все": group = "[2,3,1]"; break;
                case "Премиум": group = "[3]"; break;
                case "Медиум": group = "[2]"; break;
                case "Бомжи": group = "[1]"; break;
                case "Без бомжей": group = "[2,3]"; break;
                case "Без Медиума": group = "[3,1]"; break;
                case "Без премиума":

                    group = "[2,1]"; break;
            }
            string[] ss = _advertisingStreams.AdCompanyId.Split(",", StringSplitOptions.RemoveEmptyEntries);
            for (int h = 0; h < ss.Length; h++)
            {
                string micro1 = micro;
                if (_advertisingCompany.Where(p => p.Id.ToString() == ss[h]).First().Type == "Push")
                {

                    micro1 += $",\"active_site_groups\":{group},\"min_subscription_days\":0,\"max_subscription_days\":9999";
                }
                if (_advertisingCompany.Where(p => p.Id.ToString() == ss[h]).First().Type == "Vitrina")
                {

                    micro1 += $",\"active_site_groups\":{group}";
                }
                if (_advertisingStreams.Proxy == "True")
                {
                    micro1 += ",\"exclude_proxy_ips\":true";
                }
                else
                {
                    micro1 += ",\"exclude_proxy_ips\":false";
                }
                micro1 += "}";

                Api<bool>.PutApiRespons($"https://adv-api.adprofex.com/api/campaign/{ss[h]}", micro1, _userOptions.TokenAdprofex);
            }



        }
        string Translate(string obj)
        {
            return IsoNames.CountryNames.GetName(new CultureInfo("ru-RU"), obj);

        }

        string TranslateOs(string obj)
        {
            switch (obj)
            {
                case "ПК": return "Компьютер";
                case "Моб.": return "Мобильный телефон";
                default: return " ";
            }

        }


        public static string ChangeCountry(string item)
        {

            switch (item)
            {
                case "Аргентина": return "14";
                case "Бразилия": return "32";
                case "Андорра": return "11";
                case "Босния и Герцеговина": return "30";
                case "Гонконг": return "61";
                case "Гренландия": return "63";
                case "Венесуэла": return "43";
                case "Израиль": return "77";
                case "Мексика": return "131";
                case "Перу": return "165";
                case "Колумбия": return "101";
                case "Буркина-Фасо": return "36";
                case "Египет": return "73";
                case "Камбоджа": return "89";
                case "Канада": return "91";
                case "Малайзия": return "124";
                case "Люксембург": return "116";
                case "Марокко": return "128";
                case "Кения": return "93";
                case "Куба": return "105";
                case "Ирак": return "80";
                case "Объединенные Арабские Эмираты": return "150";
                case "Панама": return "162";
                case "Никарагуа": return "145";
                case "Гондурас": return "60";
                case "Катар": return "92";
                case "Дания": return "67";
                case "Мальта": return "127";
                case "Лихтенштейн": return "115";
                case "Монако": return "135";
                case "Норвегия": return "149";
                case "Исландия": return "82";
                case "Чили": return "233";
                case "Таджикистан": return "202";
                case "Сербия": return "190";
                case "Филиппины": return "221";
                case "Таиланд": return "203";
                case "Саудовская Аравия": return "179";
                case "Индия": return "248";
                case "Сингапур": return "191";
                case "Алжир": return "6";
                case "Эквадор": return "237";
                case "Черногория": return "231";
                case "Тунис": return "211";
                case "Австрия": return "2";
                case "Бельгия": return "24";
                case "Болгария": return "27";
                case "Венгрия": return "42";
                case "Германия": return "57";
                case "Азербайджан": return "3";
                case "Армения": return "15";
                case "Бангладеш": return "19";
                case "Беларусь": return "23";
                case "Великобритания": return "41";
                case "Вьетнам": return "46";
                case "Финляндия": return "222";
                case "Ирландия": return "249";
                case "Туркменистан": return "212";
                case "Абхазия": return "247";
                case "Грузия": return "65";
                case "Индонезия": return "78";
                case "Казахстан": return "87";
                case "Кыргызстан": return "95";
                case "Молдова": return "134";
                case "Республика Корея": return "170";
                case "США": return "200";
                case "Турция": return "213";
                case "Узбекистан": return "215";
                case "Украина": return "216";
                case "Швейцария": return "234";
                case "Южно-Африканская Республика": return "242";
                case "Греция": return "64";
                case "Испания": return "83";
                case "Сенегал": return "186";
                case "Сомали": return "197";
                case "Италия": return "84";
                case "Кипр": return "94";
                case "Латвия": return "109";
                case "Литва": return "114";
                case "Нидерланды": return "144";
                case "Польша": return "166";
                case "Португалия": return "167";
                case "Румыния": return "174";
                case "Словакия": return "194";
                case "Словения": return "195";
                case "Франция": return "224";
                case "Хорватия": return "228";
                case "Чехия": return "232";
                case "Швеция": return "235";
                case "Эстония": return "240";
                case "Коста-Рика": return "103";
                case "Нигерия": return "143";
                case "Доминика": return "70";
                case "Доминиканская Республика": return "71";
                case "Монголия": return "136";
                case "Япония": return "246";
                case "Австралия": return "1";
                case "Албания": return "5";
                case "Ангола": return "10";
                case "Афганистан": return "17";
                case "Багамы": return "18";
                case "Барбадос": return "20";
                case "Бахрейн": return "21";
                case "Бермуды": return "26";
                case "Боливия": return "28";
                case "Бутан": return "38";
                case "Ватикан": return "40";
                case "Виргинские острова (Великобритания)": return "34";
                case "Виргинские острова (США)": return "7";
                case "Внешние малые острова (США)": return "44";
                case "Гаити": return "48";
                case "Гана": return "51";
                case "Гваделупа": return "52";
                case "Иран": return "81";
                case "Иордания": return "79";
                case "Французская Полинезия": return "225";
                case "Эфиопия": return "241";
                case "Южная Осетия": return "251";
                case "Южный Судан": return "244";
                case "Ямайка": return "245";
                case "Ливан": return "112";
                case "Лаос": return "108";
                case "Оман": return "151";
                case "Либерия": return "111";
                case "Нигер": return "142";
                case "Ливия": return "113";
                case "Маврикий": return "117";
                case "Мавритания": return "118";
                case "Мьянма": return "138";
                case "Мадагаскар": return "119";
                case "Мартиника": return "129";
                case "Намибия": return "139";
                case "Непал": return "141";
                case "Пакистан": return "159";
                case "Парагвай": return "164";
                case "Руанда": return "173";
                case "Тайвань (Китай)": return "97";
                case "Танзания": return "204";
                case "Уганда": return "214";
                case "Фарерские острова": return "219";
                case "Уругвай": return "218";
                case "Фиджи": return "220";
                case "Французские Южные и Антарктические территории": return "226";
                case "Шри-Ланка": return "236";
                case "Новая Зеландия": return "147";
                case "Камерун": return "90";
                case "Замбия": return "74";
                case "Кот-д'Ивуар": return "104";
                case "Гватемала": return "53";
                case "Тринидад и Тобаго": return "209";
                case "Сальвадор": return "175";
                case "Китай": return "99";
                case "Россия": return "172";

                case "14": return "Аргентина";
                case "32": return "Бразилия";
                case "11": return "Андорра";
                case "30": return "Босния и Герцеговина";
                case "61": return "Гонконг";
                case "63": return "Гренландия";
                case "43": return "Венесуэла";
                case "77": return "Израиль";
                case "131": return "Мексика";
                case "165": return "Перу";
                case "101": return "Колумбия";
                case "36": return "Буркина-Фасо";
                case "73": return "Египет";
                case "89": return "Камбоджа";
                case "91": return "Канада";
                case "124": return "Малайзия";
                case "116": return "Люксембург";
                case "128": return "Марокко";
                case "93": return "Кения";
                case "105": return "Куба";
                case "80": return "Ирак";
                case "150": return "Объединенные Арабские Эмираты";
                case "162": return "Панама";
                case "145": return "Никарагуа";
                case "60": return "Гондурас";
                case "92": return "Катар";
                case "67": return "Дания";
                case "127": return "Мальта";
                case "115": return "Лихтенштейн";
                case "135": return "Монако";
                case "149": return "Норвегия";
                case "82": return "Исландия";
                case "233": return "Чили";
                case "202": return "Таджикистан";
                case "190": return "Сербия";
                case "221": return "Филиппины";
                case "203": return "Таиланд";
                case "179": return "Саудовская Аравия";
                case "248": return "Индия";
                case "191": return "Сингапур";
                case "6": return "Алжир";
                case "237": return "Эквадор";
                case "231": return "Черногория";
                case "211": return "Тунис";
                case "2": return "Австрия";
                case "24": return "Бельгия";
                case "27": return "Болгария";
                case "42": return "Венгрия";
                case "57": return "Германия";
                case "3": return "Азербайджан";
                case "15": return "Армения";
                case "19": return "Бангладеш";
                case "23": return "Беларусь";
                case "41": return "Великобритания";
                case "46": return "Вьетнам";
                case "222": return "Финляндия";
                case "249": return "Ирландия";
                case "212": return "Туркменистан";
                case "247": return "Абхазия";
                case "65": return "Грузия";
                case "78": return "Индонезия";
                case "87": return "Казахстан";
                case "95": return "Кыргызстан";
                case "134": return "Молдова";
                case "170": return "Республика Корея";
                case "200": return "США";
                case "213": return "Турция";
                case "215": return "Узбекистан";
                case "216": return "Украина";
                case "234": return "Швейцария";
                case "242": return "Южно-Африканская Республика";
                case "64": return "Греция";
                case "83": return "Испания";
                case "186": return "Сенегал";
                case "197": return "Сомали";
                case "84": return "Италия";
                case "94": return "Кипр";
                case "109": return "Латвия";
                case "114": return "Литва";
                case "144": return "Нидерланды";
                case "166": return "Польша";
                case "167": return "Португалия";
                case "174": return "Румыния";
                case "194": return "Словакия";
                case "195": return "Словения";
                case "224": return "Франция";
                case "228": return "Хорватия";
                case "232": return "Чехия";
                case "235": return "Швеция";
                case "240": return "Эстония";
                case "103": return "Коста-Рика";
                case "143": return "Нигерия";
                case "70": return "Доминика";
                case "71": return "Доминиканская Республика";
                case "136": return "Монголия";
                case "246": return "Япония";
                case "1": return "Австралия";
                case "5": return "Албания";
                case "10": return "Ангола";
                case "17": return "Афганистан";
                case "18": return "Багамы";
                case "20": return "Барбадос";
                case "21": return "Бахрейн";
                case "26": return "Бермуды";
                case "28": return "Боливия";
                case "38": return "Бутан";
                case "40": return "Ватикан";
                case "34": return "Виргинские острова (Великобритания)";
                case "7": return "Виргинские острова (США)";
                case "44": return "Внешние малые острова (США)";
                case "48": return "Гаити";
                case "51": return "Гана";
                case "52": return "Гваделупа";
                case "81": return "Иран";
                case "79": return "Иордания";
                case "225": return "Французская Полинезия";
                case "241": return "Эфиопия";
                case "251": return "Южная Осетия";
                case "244": return "Южный Судан";
                case "245": return "Ямайка";
                case "112": return "Ливан";
                case "108": return "Лаос";
                case "151": return "Оман";
                case "111": return "Либерия";
                case "142": return "Нигер";
                case "113": return "Ливия";
                case "117": return "Маврикий";
                case "118": return "Мавритания";
                case "138": return "Мьянма";
                case "119": return "Мадагаскар";
                case "129": return "Мартиника";
                case "139": return "Намибия";
                case "141": return "Непал";
                case "159": return "Пакистан";
                case "164": return "Парагвай";
                case "173": return "Руанда";
                case "97": return "Тайвань (Китай)";
                case "204": return "Танзания";
                case "214": return "Уганда";
                case "219": return "Фарерские острова";
                case "218": return "Уругвай";
                case "220": return "Фиджи";
                case "226": return "Французские Южные и Антарктические территории";
                case "236": return "Шри-Ланка";
                case "147": return "Новая Зеландия";
                case "90": return "Камерун";
                case "74": return "Замбия";
                case "104": return "Кот-д'Ивуар";
                case "53": return "Гватемала";
                case "209": return "Тринидад и Тобаго";
                case "175": return "Сальвадор";
                case "99": return "Китай";
                case "172": return "Россия";

                default: return "asd";
            }


        }


    }
}
