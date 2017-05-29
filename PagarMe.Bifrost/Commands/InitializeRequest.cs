using System;

namespace PagarMe.Bifrost.Commands
{
    public class InitializeRequest
    {
        public String EncryptionKey { get; set; }
        public String DeviceId { get; set; }
        public Int32 BaudRate { get; set; }
        public Boolean SimpleInitialize { get; set; }
    }
}