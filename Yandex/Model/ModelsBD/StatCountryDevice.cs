using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Yandex.WorkerClass;

namespace Yandex.Model.ModelsBD
{
    public class StatCountryDevice
    {
        public long Id { get; set; }
        
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
        public static StatCountryDevice operator +(StatCountryDevice item, YandexStat value)
        {
            using var _ = WorkWithBD<StatCountryDevice>.Read(new StatCountryDevice());
            Type type = item.GetType();
            long id = 1;
            PropertyInfo[] properties = type.GetProperties();
            try
            {
                 id = _.Result.Max(p => p.Id) + 1;
            }
            catch
            {

            }
            item.Id = id;

            
            Type type1 = value.GetType();
            PropertyInfo[] properties1 = type1.GetProperties();
            for (int j = 1; j < properties.Length; j++)
            {
                var vs = properties1[j + -1].GetValue(value);
                properties[j].SetValue(item, vs);
            }
            return item;
           
        }
    }
}
