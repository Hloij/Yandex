using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Model.ModelsBD
{
    internal class CountryOprions
    {
       public long Id { get; set; }
       public long Time { get; set; }
       public long KoefUp { get; set; }
       public long KoefDown { get; set; }
       public long KoefProc { get; set; }
       public long Min { get; set; }
       public long Max { get; set; }
       public double CPMV { get; set; }
       public double Rr { get; set; }
       public string Group { get; set; }
       public long Prosmotr { get; set; }

    }
}
