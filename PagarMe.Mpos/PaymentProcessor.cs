using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace PagarMe.Mpos
{
    public class PaymentProcessor
    {
        private readonly string _devicePath, _encryptionKey;
        private Mpos _mpos;

        public PaymentProcessor(string devicePath, string encryptionKey)
        {
            _devicePath = devicePath;
            _encryptionKey = encryptionKey;
        }

        public Task Initialize()
        {
            var port = new SerialPort(_devicePath);

            port.Open();

            _mpos = new Mpos(port.BaseStream, _encryptionKey);

            return _mpos.Initialize();
        }
    }
}

