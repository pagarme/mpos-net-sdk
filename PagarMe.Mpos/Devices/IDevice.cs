using System.IO;
using System.Threading.Tasks;

namespace PagarMe.Mpos.Devices
{
    public interface IDevice
    {
        string Id { get; }

        string Name { get; }

        string Manufacturer { get; }

        DeviceKind Kind { get; }

        Stream Open();
    }
}