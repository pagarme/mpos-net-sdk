using PagarMe.Mpos.Devices;
using System;

namespace PagarMe.Bifrost.Providers
{
    public class InitializationOptions
    {
        public IDevice Device { get; set; }
        public String EncryptionKey { get; set; }
        public String StoragePath { get; set; }
        public Int32 BaudRate { get; set; }
        public Action<Int32> OnError { get; set; }
    }
}