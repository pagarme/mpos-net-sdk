using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentTest
{
    public static class Config
    {
        static Config()
        {
            Port = ConfigurationManager.AppSettings["Port"];
            BaudRate = Int32.Parse(ConfigurationManager.AppSettings["BaudRate"]);
        }

        public readonly static String Port;
        public readonly static Int32 BaudRate;
    }
}
