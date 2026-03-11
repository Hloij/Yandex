using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Model.ModelsBD
{
    internal class AdvertisingStreams
    {
       public long Id { get; set; }
       public string Name { get; set; }
       public string AdCompanyId { get; set; }
       public long CountryOptionsId { get; set; }
       public long DeviceOptionsId { get; set; }
       public string Proxy { get; set; }

    }
}
