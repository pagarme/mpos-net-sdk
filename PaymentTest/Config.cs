using System;
using System.Configuration;

namespace PaymentTest
{
    public static class Config
    {
        static Config()
        {
            Port = ConfigurationManager.AppSettings["Port"];
            BaudRate = Int32.Parse(ConfigurationManager.AppSettings["BaudRate"]);

            SqlitePath = ConfigurationManager.AppSettings["SqlitePath"];
        }

        public readonly static String Port;
        public readonly static Int32 BaudRate;

        public readonly static String SqlitePath;
    }
}
