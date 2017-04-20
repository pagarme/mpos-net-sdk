namespace PagarMe.Mpos.Bridge.Commands
{
    public class FinishPaymentRequest
    {
        public string EmvData { get; set; }

        public bool Success { get; set; }

        public int ResponseCode { get; set; }
    }
}