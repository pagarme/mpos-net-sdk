namespace PagarMe.Mpos.Bridge.Commands
{
    public class PaymentRequest
    {
        public ProcessPaymentRequest Process { get; set; }
        public FinishPaymentRequest Finish { get; set; }

        public Type RequestType { get; set; }

        public enum Type
        {
            UnknownCommand = 0,
            Process = 1,
            Finish = 2,
        }
    }
}