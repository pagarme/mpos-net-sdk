using System;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Linq;
using PagarMe.Mpos;

namespace PaymentTest
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var port = new SerialPort("/dev/tty.usbmodem1421", 19200);

            port.Open();

            var mpos = new Mpos(port.BaseStream, "ek_live_bspDfnKtdZahowfxSxuYTdYxaaDp1v");

            mpos.Initialized += (sender, e) => mpos.ProcessPayment(250);

            mpos.PaymentProcessed += (sender, e) => {
                
            };

            mpos.Initialize();
        }
    }
}
