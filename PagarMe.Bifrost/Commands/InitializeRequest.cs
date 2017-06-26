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

        private Int32 timeoutMilliseconds;
        public Int32 TimeoutMilliseconds
        {
            get => timeoutMilliseconds;
            set => timeoutMilliseconds = value == 0 ? Timeout.Infinite : value;
        }
    }
}