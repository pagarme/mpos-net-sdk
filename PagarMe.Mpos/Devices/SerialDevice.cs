using System;
using System.IO;
using System.IO.Ports;

namespace PagarMe.Mpos.Devices
{
    internal class SerialDevice : IDevice
    {
        private SerialPort port;

        public String Port { get; set; }

        public String Id { get; set; }

        public String Name { get; set; }

        public String Manufacturer { get; set; }

        public DeviceKind Kind
        {
            get
            {
                return DeviceKind.Serial;
            }
        }

        public SerialDevice(String port)
        {
            Id = Guid.NewGuid().ToString();
            Port = port;
            Name = "Serial Device (" + port + ")";
            Manufacturer = "";
        }
        
        public Stream Open(Int32 baudRate)
        {
            if (port == null)
            {
                port = new SerialPort(Port, baudRate, Parity.None, 8, StopBits.One);
            }

            if (!port.IsOpen)
            {
                port.Open();
            }

            return port.BaseStream;
        }

        public void Close()
        {
            port.Close();
        }

        public void Dispose()
        {
            port.Dispose();
        }
    }
}