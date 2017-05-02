using System;

namespace PagarMe.Mpos.Bridge.Commands
{
    public class PaymentResponse
    {
        public String CardHash { get; internal set; }

        public String Error { get; internal set; }

        public Type ResponseType { get; internal set; }

        public enum Type
        {
            UnknownCommand = 0,
            Processed = 1,
            Finished = 2,
            Error = 3,
        }
    }
}