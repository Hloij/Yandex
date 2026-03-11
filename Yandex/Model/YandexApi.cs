using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Yandex.Model.YandexApiCountry;

namespace Yandex.Model
{
    public class YandexApiCountry
    {// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        #region YandexCountry
        public class Data
        {
            public List<Point> points { get; set; }
        }

        public class Dimensions
        {
            public string domain { get; set; }
            public string geo { get; set; }
        }

        public class Measure
        {
            public double partner_wo_nds { get; set; }
            public double cpmv_partner_wo_nds { get; set; }
            public int impressions { get; set; }
        }

        public class Point
        {
            public Dimensions dimensions { get; set; }
            public List<Measure> measures { get; set; }
        }

        public class YandexCountry
        {
            public Data data { get; set; }
        }
        #endregion

    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class YandexApiDevice
    {
        #region YandexDevice
        public class Data
        {
            public List<Point> points { get; set; }
        }

        public class Dimensions
        {
            public string domain { get; set; }
            public string device { get; set; }
        }

        public class Measure
        {
            public double partner_wo_nds { get; set; }
            public double cpmv_partner_wo_nds { get; set; }
            public int impressions { get; set; }
        }

        public class Point
        {
            public Dimensions dimensions { get; set; }
            public List<Measure> measures { get; set; }
        }

        public class YandexDevice
        {
            public Data data { get; set; }
        }
        #endregion
    }
}
