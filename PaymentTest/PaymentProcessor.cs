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
            _port = new SerialPort(device, 240000, Parity.None, 8, StopBits.One);
            _port.Open();

            _mpos = new Mpos(_port.BaseStream, "ek_live_bspDfnKtdZahowfxSxuYTdYxaaDp1v");
            _mpos.NotificationReceived += (sender, e) => Console.WriteLine("Status: {0}", e);
			_mpos.TableUpdated += (sender, e) => Console.WriteLine("LOADED: {0}", e);
			_mpos.Errored += (sender, e) => Console.WriteLine ("I GOT ERROR {0}", e);
			_mpos.PaymentProcessed += (sender, e) => Console.WriteLine("HEY CARD HASH " + e.CardHash);
			_mpos.FinishedTransaction += (sender, e) => Console.WriteLine ("FINISHED TRANSACTION!");

            //PagarMeService.DefaultApiEndpoint = "http://localhost:3000";
            PagarMeService.DefaultEncryptionKey = "ek_live_bspDfnKtdZahowfxSxuYTdYxaaDp1v";
            PagarMeService.DefaultApiKey = "ak_live_JPHX33BR4omHj3ewCEghXsh12BH8VG";
        }

        public async Task Initialize()
        {
            await _mpos.Initialize();

            await _mpos.SynchronizeTables(false);
			_mpos.Display("Hello, world!");
        }

        public async Task Pay(int amount)
        {
            var result = await _mpos.ProcessPayment(amount);
			Console.WriteLine (result.CardHash);

            var transaction = new Transaction
                {
                    CardHash = result.CardHash,
                    Amount = amount,
                    ShouldCapture = false
                };

            await transaction.SaveAsync();

			Console.WriteLine (transaction);
			Console.WriteLine ("Transaction ARC = " + transaction.AcquirerResponseCode + ", Id = " + transaction.Id);
			Console.WriteLine ("ACQUIRER RESPONSE CODE = " + transaction.AcquirerResponseCode);
			int x = Int32.Parse (transaction.AcquirerResponseCode);
			object obj = transaction["card_emv_response"];
			string response = obj == null ? null : obj.ToString (); 

			await _mpos.FinishTransaction(Int32.Parse(transaction.AcquirerResponseCode), response);

        }
    }
}

