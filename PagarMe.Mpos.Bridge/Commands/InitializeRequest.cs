using System;

namespace PagarMe.Mpos.Bridge.Commands
{
    public class InitializeRequest
    {
        public String EncryptionKey { get; set; }
        public String DeviceId { get; set; }
        public Int32 BaudRate { get; set; }
    }
}