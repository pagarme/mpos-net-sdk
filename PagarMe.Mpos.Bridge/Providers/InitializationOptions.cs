using PagarMe.Mpos.Devices;

namespace PagarMe.Mpos.Bridge.Providers
{
    public class InitializationOptions
    {
        public IDevice Device { get; set; }
        public string EncryptionKey { get; set; }
        public string StoragePath { get; set; }
    }
}