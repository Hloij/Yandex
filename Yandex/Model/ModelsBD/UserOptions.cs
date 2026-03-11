using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Model.ModelsBD
{
    internal class UserOptions
    {
       public long Id { get; set; }
       public string TokenYandex { get;set; }
       public string TokenAdprofex { get;set; }
       public string AdprofexMail { get;set; }
       public string AdProfexPassword { get;set; }
       public string Domen { get;set; }
        
    }
}
