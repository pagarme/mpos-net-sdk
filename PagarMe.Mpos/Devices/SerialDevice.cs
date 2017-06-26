using System;
using System.IO;
using System.IO.Ports;

namespace PagarMe.Mpos.Devices
{
    internal class SerialDevice : IDevice
    {
        private SerialPort _port;

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
            if (_port == null)
            {
                _port = new SerialPort(Port, baudRate, Parity.None, 8, StopBits.One);
            }

            if (!_port.IsOpen)
            {
                _port.Open();
            }

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