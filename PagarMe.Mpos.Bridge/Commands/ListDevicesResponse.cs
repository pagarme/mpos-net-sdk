using PagarMe.Mpos.Devices;

namespace PagarMe.Mpos.Bridge.Commands
{
    public class ListDevicesResponse
    {
        public IDevice[] Devices { get; set; }
    }
}