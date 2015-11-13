using System;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Linq;
using PagarMe;
using PagarMe.Mpos;

namespace PaymentTest
{
    public class PaymentProcessor
    {
        private readonly SerialPort _port;
        private readonly Mpos _mpos;

        public PaymentProcessor(string device)
        {
            _port = new SerialPort(device, 19200, Parity.None, 8, StopBits.One);
            _port.Open();

			_mpos = new Mpos(_port.BaseStream, "ek_live_mXl4E5lajeE7i3udVsQuoCz7PaYf9s");
            _mpos.NotificationReceived += (sender, e) => Console.WriteLine("Status: {0}", e);

           // PagarMeService.DefaultApiEndpoint = "http://localhost:3000";
            //PagarMeService.DefaultEncryptionKey = "ek_live_IiZGjjXdxDug8t8xRtEFas0dke6I7H";
            //PagarMeService.DefaultApiKey = "ak_live_SIfpRudJkS04ga5pQxag8Sz8Fvdr4z";
        }

        public async Task Initialize()
        {
            await _mpos.Initialize();

            await _mpos.SynchronizeTables();
			//_mpos.Display("Hello, world!");
        }

        public async Task Pay(int amount)
        {
            var result = await _mpos.ProcessPayment(amount);
			Console.WriteLine (result.CardHash);

            /*var transaction = new Transaction
                {
                    CardHash = result.CardHash,
                    Amount = amount,
                    ShouldCapture = false
                };

            await transaction.SaveAsync();

            await _mpos.FinishTransaction(Int32.Parse(transaction.AcquirerResponseCode), transaction["card_emv_response"].ToString());
            */
        }
    }
}

