using System;
using System.IO;
using System.IO.Ports;

namespace PagarMe.Mpos.Devices
{
    internal class SerialDevice : IDevice
    {
        private SerialPort _port;

        public string Port { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Manufacturer { get; set; }

        public DeviceKind Kind
        {
            get
            {
                return DeviceKind.Serial;
            }
        }

        public SerialDevice(string port, int baudRate)
        {
            _port = new SerialPort(port, baudRate, Parity.None, 8, StopBits.One);

            Id = Guid.NewGuid().ToString();
            Port = port;
            Name = "Serial Device (" + port + ")";
            Manufacturer = "";
        }
        
        public Stream Open()
        {
            _port.Open();

            return _port.BaseStream;
        }

        public void Close()
        {
            _port.Close();
        }

        public void Dispose()
        {
            _port.Dispose();
        }
    }
}