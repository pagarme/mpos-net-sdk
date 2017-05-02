using System;
using System.Configuration;

namespace PagarMe.Generic
{
    public static class Config
    {
        static Config()
        {
            Port = ConfigurationManager.AppSettings["Port"];
            BaudRate = Int32.Parse(ConfigurationManager.AppSettings["BaudRate"]);

            EncryptionKey = ConfigurationManager.AppSettings["EncryptionKey"];
            ApiKey = ConfigurationManager.AppSettings["ApiKey"];

            SqlitePath = ConfigurationManager.AppSettings["SqlitePath"];
        }

        public readonly static String Port;
        public readonly static Int32 BaudRate;

        public readonly static String EncryptionKey;
        public readonly static String ApiKey;

        public readonly static String SqlitePath;
    }
}
