using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Build_Fetcher.config
{
    /// <summary>
    /// This is the class that contains configuration properities
    /// </summary>
    public class ConfigJson
    {
        public bool NeedInstall { get; set; }
        public string Directory { get; set; }
        public Product Product { get; set; }
        public bool AutoHide { get; set; }
        public string PreviousDrop { get; set; }
        public string AutoLogonUserName { get; set; }
        public string AutoLogonPassword { get; set; }
        public ConfigJson()
        {
            NeedInstall = false;
            Directory = "";
            Product = Product.Unspecified;
            AutoHide = false;
            PreviousDrop = "";
            AutoLogonUserName = "";
            AutoLogonPassword = "";
        }
    }
}
