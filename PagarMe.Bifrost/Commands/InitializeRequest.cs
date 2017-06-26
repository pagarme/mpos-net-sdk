using System;
using System.Threading;

namespace PagarMe.Bifrost.Commands
{
    public class InitializeRequest
    {
        public String EncryptionKey { get; set; }
        public String DeviceId { get; set; }
        public Int32 BaudRate { get; set; }
        public Boolean SimpleInitialize { get; set; }

        private Int32 timeout;
        public Int32 TimeoutMilliseconds
        {
            get => timeout == 0 ? Timeout.Infinite : timeout;
            set => timeout = value;
        }
    }
}