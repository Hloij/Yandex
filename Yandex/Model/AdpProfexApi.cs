using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Model
{
    public class AdpProfexApiCountry
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Datum
        {
            public string campaign { get; set; }
            public string country { get; set; }
            public string buy_count { get; set; }
            public int click_count { get; set; }
            public double dsp_flow { get; set; }
            public double click_price_dsp { get; set; }
            public int conversion_cost { get; set; }
            public int conversion_count { get; set; }
            public int cpa_conversion_count { get; set; }
            public double ctr { get; set; }
            public int conversion_rate { get; set; }
            public bool? summary { get; set; }
            public static Datum operator +(Datum datum, Datum datum1)
            {
                datum.buy_count = (Convert.ToInt64(datum.buy_count) + Convert.ToInt64(datum1.buy_count)).ToString();
                datum.click_count += datum1.click_count;
                datum.click_price_dsp = Math.Round((datum1.click_price_dsp + datum.click_price_dsp) / 2, 2);
                datum.dsp_flow += datum1.dsp_flow;
                return datum;
            }
        }

        public class Link
        {
            public string url { get; set; }
            public string label { get; set; }
            public bool active { get; set; }
        }

        public class ApiCountry
        {
            public int current_page { get; set; }
            public List<Datum> data { get; set; }
            public string first_page_url { get; set; }
            public int from { get; set; }
            public int last_page { get; set; }
            public string last_page_url { get; set; }
            public List<Link> links { get; set; }
            public object next_page_url { get; set; }
            public string path { get; set; }
            public int per_page { get; set; }
            public object prev_page_url { get; set; }
            public int to { get; set; }
            public int total { get; set; }
        }
        


    }
    public class AdpProfexApiDevice
    {
        public class Datum
        {
            public string campaign { get; set; }
            public string os { get; set; }
            public string buy_count { get; set; }
            public int click_count { get; set; }
            public double dsp_flow { get; set; }
            public double click_price_dsp { get; set; }
            public int conversion_cost { get; set; }
            public int conversion_count { get; set; }
            public int cpa_conversion_count { get; set; }
            public double ctr { get; set; }
            public double conversion_rate { get; set; }
            public bool? summary { get; set; }
            public static Datum operator +(Datum datum, Datum datum1)
            {
                datum.buy_count = (Convert.ToInt64(datum.buy_count)+ Convert.ToInt64(datum1.buy_count)).ToString();
                datum.click_count += datum1.click_count;
                datum.click_price_dsp = Math.Round((datum1.click_price_dsp + datum.click_price_dsp) / 2, 2);
                datum.dsp_flow += datum1.dsp_flow;
                return datum;
            }
        }

        public class Link
        {
            public string url { get; set; }
            public string label { get; set; }
            public bool active { get; set; }
        }

        public class ApiDevice
        {
            public int current_page { get; set; }
            public List<Datum> data { get; set; }
            public string first_page_url { get; set; }
            public int from { get; set; }
            public int last_page { get; set; }
            public string last_page_url { get; set; }
            public List<Link> links { get; set; }
            public object next_page_url { get; set; }
            public string path { get; set; }
            public int per_page { get; set; }
            public object prev_page_url { get; set; }
            public int to { get; set; }
            public int total { get; set; }
        }
    }
    public class CompaingFields
    {
        public string[] groups { get; set; }
        public Fields[] fields { get; set; }
    }
    public class Fields
    {
        public Fields()
        {
            name = "";
            values = new string[0];
        }
        public string name { get; set; }
        public string[] values { get; set; }
    }
    public class PreparationForRequst()
    {
        public static CompaingFields InfoForZaprosAdFox(string[] values, string compan, string objectfinder)//объект для запроса
        {

            if (objectfinder == null || objectfinder == "") objectfinder = "country";
            Fields fields = new Fields();
            fields.name = "date";
            fields.values = values;
            string[] values1 = new string[1];
            values1[0] = compan;



            Fields fields1 = new Fields();
            fields1.name = "campaign";
            fields1.values = values1;
            Fields[] das = { fields, fields1 };
            string[] groups = { "campaign", $"{objectfinder}" };//формирование сортировки для выборки

            CompaingFields compaingFields = new CompaingFields();
            compaingFields.groups = groups;
            compaingFields.fields = das;
            return compaingFields;
        }
    }
    public class ApiInfoCompainCPC
    {
        public CampaignMicroBidding campaign_micro_bidding { get; set; }
    }
    public class CampaignMicroBidding
    {
        [JsonProperty("2")]
        public List<Micro2> _2 { get; set; }

        [JsonProperty("1")]
        public List<Micro1> _1 { get; set; }

        [JsonProperty("3")]
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.
        public List<Micro3> _3 { get; set; }
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.
    }
    public class Micro1
    {
        public string id { get; set; }
        public int coeff { get; set; }
    }

    public class Micro2
    {
        public string id { get; set; }
        public int coeff { get; set; }
    }

    public class Micro3
    {
        public string id { get; set; }
        public int coeff { get; set; }
    }
    public class MicroCountry
    {//"id":"1","coeff":120

        public MicroCountry(string param)
        {
            param = param.Replace("\"", "");
            param = param.Replace("coeff:", "");
            param = param.Replace("id:", "");
            string[] split = param.Split(',', StringSplitOptions.RemoveEmptyEntries);
            id = split[0];
            coeff = split[1];
        }
        public string id { get; set; }
        public string coeff { get; set; }
    }
}
