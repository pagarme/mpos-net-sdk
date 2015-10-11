using System;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Linq;
using PagarMe.Mpos;

namespace PaymentTest
{
    public class PaymentProcessor
    {
        private readonly SerialPort _port;
        private readonly Mpos _mpos;

        public PaymentProcessor()
        {
            _port = new SerialPort("/dev/tty.usbmodem89", 19200);
            _port.Open();

            _mpos = new Mpos(_port.BaseStream, "ek_live_IiZGjjXdxDug8t8xRtEFas0dke6I7H");
        }

        public async Task Initialize()
        {
            await _mpos.Initialize();
        }

        public async Task Pay(int amount)
        {
            PaymentResult result;

            await Initialize();

            result = await _mpos.ProcessPayment(amount, PaymentFlags.None);

        }
    }
}

