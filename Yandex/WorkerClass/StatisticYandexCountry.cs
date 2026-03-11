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
        YandexApiCountry.YandexCountry _apiContryYandex = new();
        YandexApiDevice.YandexDevice _apiDeviceYandex = new();
        List<AdpProfexApiCountry.ApiCountry> _apiCountryAdprofex = new();
        List<AdpProfexApiDevice.ApiDevice> _apiDeviceAdprofex = new();
        bool _zero;
        string _name;
        UserOptions _userOptions;
        AdvertisingStreams _advertisingStreams;
        List<AdvertisingCompany> _advertisingCompany;
        DeviceOptions _deviceOptions;
        CountryOprions _countryOptions;
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
            List<UserOptions> user = await WorkWithBD<UserOptions>.Read(new UserOptions());
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
        async Task GetStatLastAsync()
        {
            List<StatCountryDevice> LastStat1 = await WorkWithBD<StatCountryDevice>.Read(new StatCountryDevice());
            List<YandexStat> LastStat = new List<YandexStat>();


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
            for (int j = 0; j < _apiContryYandex.data.points.Count; j++)
            {
                for (int h = 0; h < _apiCountryAdprofex.Count; h++)
                {
                    bool was = true;
                    for (int i = 0; i < _apiCountryAdprofex[h].data.Count - 1; i++)
                    {
                        string country = Translate(_apiCountryAdprofex[h].data[i].country);
                        if (_apiContryYandex.data.points[j].dimensions.geo == country)
                        {
                            if (_statCountryNow.FirstOrDefault(p => p.Name == country) == null)
                            {
                                YandexStat statCountryDevice = new();
                                statCountryDevice.Domen = _apiContryYandex.data.points[j].dimensions.domain;
                                statCountryDevice.Name = _apiContryYandex.data.points[j].dimensions.geo;
                                statCountryDevice.CPMV = _apiContryYandex.data.points[j].measures[0].cpmv_partner_wo_nds;
                                statCountryDevice.Dohod = _apiContryYandex.data.points[j].measures[0].partner_wo_nds;
                                statCountryDevice.Prosmotr = _apiContryYandex.data.points[j].measures[0].impressions;
                                statCountryDevice.Click = Convert.ToInt64(_apiCountryAdprofex[h].data[i].buy_count);
                                statCountryDevice.CPC = Convert.ToDouble(_apiCountryAdprofex[h].data[i].click_price_dsp);
                                statCountryDevice.Click = Convert.ToInt64(_apiCountryAdprofex[h].data[i].click_count);
                                statCountryDevice.Date = DateTime.Now;
                                statCountryDevice.Rr = _apiContryYandex.data.points[j].measures[0].partner_wo_nds / _apiCountryAdprofex[h].data[i].dsp_flow;
                                statCountryDevice.Rashod = _apiCountryAdprofex[h].data[i].dsp_flow;
                                statCountryDevice.DohodClear = _apiContryYandex.data.points[j].measures[0].partner_wo_nds - _apiCountryAdprofex[h].data[i].dsp_flow;
                                string id = statCountryDevice.Name;
                                ChangeCountry(ref id);
                                statCountryDevice.Kof = Micros1.Item2.campaign_micro_bidding._3.Where(p => p.id == id).First().coeff;
                                _statCountryNow.Add(statCountryDevice);
                                was = false;
                                break;
                            }
                        }
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
                            string id = statCountryDevice.Name;
                            ChangeCountry(ref id);
                            try
                            {
                                statCountryDevice.Kof = Micros1.Item2.campaign_micro_bidding._3.Where(p => p.id == id).First().coeff;
                                _statCountryNow.Add(statCountryDevice);
                            }
                            catch
                            {
                                continue;
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
                            string id = statCountryDevice.Name;
                            ChangeCountry(ref id);
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
                                    if (vs.Prosmotr <= this._countryOptions.Prosmotr)//проверяем показы если было меньше показов чем в настройках проходим дальше
                                    {
                                        if ((this._statCountryNow[i].Kof + _countryOptions.KoefUp) < _countryOptions.Max)// если коэф + коэф увеличения меньше максимального для нулей проходим дальше
                                        {
                                            this.Null.Add(vs.Name);//добавляем в нули 
                                            this._statCountryNow[i].Kof += this._countryOptions.KoefUp;
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
                                    if (vs.Prosmotr <= this._deviceOptions.Prosmotr)//проверяем показы если было меньше показов чем в настройках проходим дальше
                                    {
                                        if ((this._statDeviceNow[i].Kof + _deviceOptions.KoefUp) < _deviceOptions.Max)// если коэф + коэф увеличения меньше максимального для нулей проходим дальше
                                        {
                                            this.Null.Add(vs.Name);//добавляем в нули 
                                            this._statDeviceNow[i].Kof += this._deviceOptions.KoefUp;
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
            GetStatLastAsync();
            await Sravnenie();
            await ChangeKoefAsync();
        }
        async Task Save()
        {
            for (int i = 0; i < _statCountryNow.Count; i++)
            {

                await WorkWithBD<StatCountryDevice>.Add(new StatCountryDevice() + _statCountryNow[i]);
            }
            for (int i = 0; i < _statDeviceNow.Count; i++)
            {
                await WorkWithBD<StatCountryDevice>.Add(new StatCountryDevice() + _statDeviceNow[i]);
            }

        }
        async Task ChangeKoefAsync()
        {
            List<string> _Up = new();
            for (int i = 0; i < Up.Count; i++)
            {
                string vs = Up[i];
                ChangeCountry(ref vs);
                _Up.Add(vs);
            }
            List<string> _Down = new();
            for (int i = 0; i < Down.Count; i++)
            {
                string vs = Down[i];
                ChangeCountry(ref vs);
                _Down.Add(vs);
            }
            List<string> _Nuls = new();
            for (int i = 0; i < Null.Count; i++)
            {
                string vs = Null[i];
                ChangeCountry(ref vs);
                _Nuls.Add(vs);
            }
            for (int i = 0; i < _Up.Count; i++)
            {
                for (int j = 0; j < this.Micros1.Item2.campaign_micro_bidding._3.Count; j++)
                {

                    if ((this.Micros1.Item2.campaign_micro_bidding._3[j].id == Convert.ToString(_Up[i])) && (Convert.ToInt32(this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) > 1))
                    {
                        this.Micros1.Item2.campaign_micro_bidding._3[j].coeff = (int)(Convert.ToInt32(this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) + (this._countryOptions.KoefUp));
                    }

                }
            }
            for (int i = 0; i < _Nuls.Count; i++)
            {
                for (int j = 0; j < this.Micros1.Item2.campaign_micro_bidding._3.Count; j++)
                {

                    if (this.Micros1.Item2.campaign_micro_bidding._3[j].id == Convert.ToString(_Nuls[i]))
                    {
                        if ((this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) >= (this._countryOptions.Min) && (this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) < (this._countryOptions.Max))
                        {
                            this.Micros1.Item2.campaign_micro_bidding._3[j].coeff = (Convert.ToInt32(this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) + (int)(this._countryOptions.KoefUp));
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
                        if ((this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) - (this._countryOptions.KoefDown) > 1)
                        {
                            this.Micros1.Item2.campaign_micro_bidding._3[j].coeff = (Convert.ToInt32(this.Micros1.Item2.campaign_micro_bidding._3[j].coeff) - (int)(this._countryOptions.KoefDown));
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
                case "Всё": group = "[2,3,1]"; break;
                case "Премиум": group = "[3]"; break;
                case "Медиум": group = "[2]"; break;
                case "Бомжи": group = "[1]"; break;
                case "Без бомжей": group = "[2,3]"; break;
                case "Без Медиума": group = "[3,1]"; break;
                case "Без премиума":

                    group = "[2,1]"; break;
            }
            if (_advertisingCompany.Where(p => p.Name.Contains(_advertisingStreams.Name)).First().Type == "Push")
            {
                micro = micro.Remove(micro.Length - 1);
                micro += $",\"active_site_groups\":{group},\"min_subscription_days\":0,\"max_subscription_days\":9999";
            }
            if (_advertisingCompany.Where(p => p.Name.Contains(_advertisingStreams.Name)).First().Type == "Vitrina")
            {
                micro = micro.Remove(micro.Length - 1);
                micro += $",\"active_site_groups\":{group}";
            }
            if (_advertisingStreams.Proxy == "True")
            {
                micro += ",\"exclude_proxy_ips\":true";
            }
            else
            {
                micro += ",\"exclude_proxy_ips\":false";
            }
            micro += "}";
            string[] ss = _advertisingStreams.AdCompanyId.Split(",", StringSplitOptions.RemoveEmptyEntries);
            for (int h = 0; h < ss.Length; h++)
            {
                Api<bool>.PutApiRespons($"https://adv-api.adprofex.com/api/campaign/{ss[h]}", micro, _userOptions.TokenAdprofex);
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
                string item = changed.Name;
                ChangeCountry(ref item);
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
                case "Всё": group = "[2,3,1]"; break;
                case "Премиум": group = "[3]"; break;
                case "Медиум": group = "[2]"; break;
                case "Бомжи": group = "[1]"; break;
                case "Без бомжей": group = "[2,3]"; break;
                case "Без Медиума": group = "[3,1]"; break;
                case "Без премиума":

                    group = "[2,1]"; break;
            }
            if (_advertisingCompany.Where(p => p.Name.Contains(_advertisingStreams.Name)).First().Type == "Push")
            {
                micro = micro.Remove(micro.Length - 1);
                micro += $",\"active_site_groups\":{group},\"min_subscription_days\":0,\"max_subscription_days\":9999,";
            }
            if (_advertisingCompany.Where(p => p.Name.Contains(_advertisingStreams.Name)).First().Type == "Vitrina")
            {
                micro = micro.Remove(micro.Length - 1);
                micro += $",\"active_site_groups\":{group},";
            }
            if (_advertisingStreams.Proxy == "True")
            {
                micro += ",\"exclude_proxy_ips\":true" + "}";
            }
            else
            {
                micro += ",\"exclude_proxy_ips\":false" + "}";
            }
            await Task.Delay(1000);
            string[] ss = _advertisingStreams.AdCompanyId.Split(",", StringSplitOptions.RemoveEmptyEntries);
            for (int h = 0; h < ss.Length; h++)
            {
                Api<bool>.PutApiRespons($"https://adv-api.adprofex.com/api/campaign/{ss[h]}", micro, _userOptions.TokenAdprofex);
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

        public static void ChangeCountry(ref string item)
        {

            switch (item)
            {//добавить все остальные страны
                case "Кыргызстан": item = "95"; break;
                case "Словакия": item = "194"; break;
                case "Швейцария": item = "234"; break;
                case "Беларусь": item = "23"; break;
                case "Молдова": item = "134"; break;
                case "Узбекистан": item = "215"; break;
                case "Россия": item = "172"; break;
                case "Казахстан": item = "87"; break;
                case "Армения": item = "15"; break;
                case "Польша": item = "166"; break;
                case "Болгария": item = "27"; break;
                case "Венгрия": item = "42"; break;
                case "Германия": item = "57"; break;
                case "Греция": item = "64"; break;
                case "Испания": item = "83"; break;
                case "Италия": item = "84"; break;
                case "Латвия": item = "109"; break;
                case "Португалия": item = "167"; break;
                case "Румыния": item = "174"; break;
                case "Украина": item = "216"; break;
                case "Франция": item = "224"; break;
                case "Чешская Республика": item = "232"; break;
                case "Эстония": item = "240"; break;
                case "Литва": item = "114"; break;
                case "Грузия": item = "65"; break;
                case "Азербайджан": item = "3"; break;
                case "Филиппины": item = "221"; break;
                case "Сербия": item = "190"; break;
                case "Мексика": item = "131"; break;
                case "Индонезия": item = "78"; break;
                case "Марокко": item = "128"; break;
                case "Перу": item = "165"; break;
                case "Саудовская Аравия": item = "179"; break;
                case "Сингапур": item = "191"; break;
                case "Аргентина": item = "14"; break;
                case "Индия": item = "248"; break;
                case "Колумбия": item = "101"; break;
                case "Малайзия": item = "124"; break;
                case "Босния и Герцеговина": item = "30"; break;
                case "Чили": item = "233"; break;
                case "Ирак": item = "80"; break;
                case "Вьетнам": item = "46"; break;
                case "Израиль": item = "77"; break;
                case "Турция": item = "213"; break;
                case "Австрия": item = "2"; break;
                case "США": item = "200"; break;
                case "Таиланд": item = "203"; break;

                case "Великобритания": item = "41"; break;
                case "Канада": item = "91"; break;
                case "Финляндия": item = "222"; break;
                case "Швеция": item = "235"; break;

                case "Абхазия": item = "247"; break;
                case "Египет": item = "73"; break;
                case "Туркмения": item = "212"; break;
                case "Австралия": item = "1"; break;

            }


        }
        public static string ChangeCountry(string item)
        {

            switch (item)
            {
                case "95": return "Кыргызстан";
                case "194": return "Словакия";
                case "234": return "Швейцария";
                case "23": return "Беларусь";
                case "134": return "Молдова";
                case "215": return "Узбекистан";
                case "172": return "Россия";
                case "87": return "Казахстан";
                case "15": return "Армения";
                case "166": return "Польша";
                case "27": return "Болгария";
                case "42": return "Венгрия";
                case "57": return "Германия";
                case "64": return "Греция";
                case "83": return "Испания";
                case "84": return "Италия";
                case "109": return "Латвия";
                case "167": return "Португалия";
                case "174": return "Румыния";
                case "216": return "Украина";
                case "224": return "Франция";
                case "232": return "Чешская Республика";
                case "240": return "Эстония";
                case "114": return "Литва";
                case "65": return "Грузия";
                case "3": return "Азербайджан";
                case "221": return "Филиппины";
                case "190": return "Сербия";
                case "131": return "Мексика";
                case "78": return "Индонезия";
                case "128": return "Марокко";
                case "165": return "Перу";
                case "179": return "Саудовская Аравия";
                case "191": return "Сингапур";
                case "14": return "Аргентина";
                case "248": return "Индия";
                case "101": return "Колумбия";
                case "124": return "Малайзия";
                case "30": return "Босния и Герцеговина";
                case "233": return "Чили";
                case "80": return "Ирак";
                case "46": return "Вьетнам";
                case "77": return "Израиль";
                case "213": return "Турция";
                case "2": return "Австрия";
                case "200": return "США";
                case "203": return "Таиланд";
                case "41": return "Великобритания";
                case "91": return "Канада";
                case "222": return "Финляндия";
                case "235": return "Швеция";

                case "247": return "Абхазия";
                case "73": return "Египет";
                case "212": return "Туркмения";
                case "1": return "Австралия";
                default: return "asd";
            }


        }


    }
}
