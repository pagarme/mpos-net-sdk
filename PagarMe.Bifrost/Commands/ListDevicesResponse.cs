using PagarMe.Mpos.Devices;

namespace PagarMe.Bifrost.Commands
{
    public class ListDevicesResponse
    {
        public IDevice[] Devices { get; set; }
    }
}