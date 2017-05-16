namespace PagarMe.Bifrost.Commands
{
    public class StatusResponse
    {
        public ContextStatus Status { get; set; }

        public string ConnectedDeviceId { get; set; }

        public int AvailableDevices { get; set; }
    }
}