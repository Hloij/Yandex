using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Yandex.Model.ModelsBD;

namespace Yandex.Model
{

    public class YandexStat
    {
        public double Rashod { get; set; }
        public double Dohod { get; set; }
        public double DohodClear { get; set; }
        public long Click { get; set; }
        public long Prosmotr { get; set; }
        public long Kof { get; set; }
        public double CPMV { get; set; }
        public double CPC { get; set; }
        public double Rr { get; set; }
        public string Domen { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public static YandexStat operator +(YandexStat item, StatCountryDevice value)
        {

            Type type = item.GetType();
            PropertyInfo[] properties = type.GetProperties();
            Type type1 = value.GetType();
            PropertyInfo[] properties1 = type1.GetProperties();
            for (int j = 0; j < properties.Length; j++)
            {
                var vs = properties1[j + 1].GetValue(value);
                properties[j].SetValue(item, vs);
            }
            return item;
        }
        public static YandexStat operator +(YandexStat item, YandexStat value)
        {
            YandexStat yandexStat = new YandexStat();
            yandexStat.Name = item.Name;
            yandexStat.Rashod = item.Rashod - value.Rashod;
            if (item.DohodClear > value.DohodClear)
            {
                yandexStat.DohodClear = Math.Abs(item.DohodClear - value.DohodClear);

            }
            else
            {
                yandexStat.DohodClear = -Math.Abs(item.DohodClear - value.DohodClear);
            }
            yandexStat.Dohod = item.Dohod - value.Dohod;
            yandexStat.Kof = item.Kof - value.Kof;
            yandexStat.CPMV = item.CPMV - value.CPMV;
            yandexStat.Click = item.Click - value.Click;
            yandexStat.Prosmotr = item.Prosmotr - value.Prosmotr;
            yandexStat.CPC = item.CPC - value.CPC;
            yandexStat.Rr = item.Rr - value.Rr;


            return (yandexStat);
        }
        public static YandexStat operator +(YandexStat item, YandexApiCountry.YandexCountry value)
        {

            Type type = item.GetType();
            PropertyInfo[] properties = type.GetProperties();
            for (int j = 0; j < properties.Length; j++)
            {
                properties[j].SetValue(item, properties[j].GetValue(value));
            }
            return item;
        }
    }
}
