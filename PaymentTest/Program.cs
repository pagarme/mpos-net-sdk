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
            var port = new SerialPort("/dev/tty.usbmodem14121", 19200);

            port.Open();

            var mpos = new Mpos(port.BaseStream, "ek_live_bspDfnKtdZahowfxSxuYTdYxaaDp1v");

            mpos.OperationCompleted += (sender, e) => {

            };

            mpos.NotificationReceived += (sender, e) => {

            };

            mpos.Initialized += (sender, e) => {
                mpos.ProcessPayment(250, PaymentFlags.None);
                //mpos.Display("LOL WTF BBQ");
            };

            mpos.PaymentProcessed += (sender, e) => {
                
            };

            mpos.Initialize();

            Console.ReadLine();
        }
    }
}
