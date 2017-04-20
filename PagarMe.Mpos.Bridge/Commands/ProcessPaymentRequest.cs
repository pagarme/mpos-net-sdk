using System.Collections.Generic;

namespace PagarMe.Mpos.Bridge.Commands
{
    public class ProcessPaymentRequest
    {
        public int Amount { get; set; }

        public IEnumerable<EmvApplication> Applications { get; set; }

        public PaymentMethod MagstripePaymentMethod { get; set; }
    }
}