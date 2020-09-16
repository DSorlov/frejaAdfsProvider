using com.sorlov.eidprovider.frejaeid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.sorlov.frejaadfsprovider
{
    [Serializable]
    public class FrejaEIDConfiguration
    {
        public string SupportEmail { get; set; }
        public string CompanyName { get; set; }

        public string Endpoint { get; set; }
        public string Ca_cert { get; set; }
        public string Client_cert { get; set; }
        public string Password { get; set; }
        public string Id_type { get; set; }
        public string Attribute_list { get; set; }
        public string Minimum_level { get; set; }
        public string Default_country { get; set; }
        public string Enviroment { get; set; }
    }
}
