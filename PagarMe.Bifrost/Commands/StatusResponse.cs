namespace PagarMe.Bifrost.Commands
{
    public class StatusResponse
    {
        public ContextStatus Code { get; set; }

        public string ConnectedDeviceId { get; set; }

        public int AvailableDevices { get; set; }
    }
}