namespace PagarMe.Mpos.Bridge.Commands
{
    public class InitializeRequest
    {
        public string EncryptionKey { get; set; }

        public string DeviceId { get; set; }
    }
}