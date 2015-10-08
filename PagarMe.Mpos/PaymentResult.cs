using System;

namespace PagarMe.Mpos
{
    public class PaymentResult
    {
        public string CardHash { get; private set; }
        public PaymentStatus Status { get; internal set; }

        internal string Pan { get; set; }
        internal string Track2 { get; set; }
        internal string EmvData { get; set; }
        internal string ExpirationDate { get; set; }

        internal void CalculateCardHash(string encryptionKey)
        {
            
        }
    }
}

