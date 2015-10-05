using System;

namespace PagarMe.Mpos
{
    public class PaymentResult
    {
        private readonly PaymentStatus _status;
        private readonly string _cardHash;

        public PaymentStatus Status { get { return _status; } }
        public string CardHash { get { return _cardHash; } }

        public PaymentResult(PaymentStatus status, string cardHash)
        {
            _status = status;
            _cardHash = cardHash;
        }
    }
}

