using PagarMe.Mpos.Devices;
using System;

namespace PagarMe.Mpos.Bridge.Providers
{
    public class InitializationOptions
    {
        public IDevice Device { get; set; }
        public String EncryptionKey { get; set; }
        public String StoragePath { get; set; }
        public Int32 BaudRate { get; set; }
    }
}