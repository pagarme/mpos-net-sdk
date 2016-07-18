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
            _port = new SerialPort(device, 140000, Parity.None, 8, StopBits.One);
            _port.Open();

			_mpos = new Mpos(_port.BaseStream, "ak_test_RtRb4h796ZKnuyRF8ktDI4ovTjOkeu", "ek_test_DSO5EqJemg8fA6bFtDU5aSj4YA7tP0", "/tmp/");
			//_mpos = new Mpos(_port.BaseStream, "ek_test_UT6AN4fDN3BCUgo6kxUiOq6S20dbKc");
            _mpos.NotificationReceived += (sender, e) => Console.WriteLine("Status: {0}", e);
			_mpos.TableUpdated += (sender, e) => Console.WriteLine("LOADED: {0}", e);
			_mpos.Errored += (sender, e) => Console.WriteLine ("I GOT ERROR {0}", e);
			_mpos.PaymentProcessed += (sender, e) => Console.WriteLine("HEY CARD HASH " + e.CardHash);
			_mpos.FinishedTransaction += (sender, e) => Console.WriteLine ("FINISHED TRANSACTION!");

            PagarMeService.DefaultApiEndpoint = "http://192.168.64.2:3000";
			PagarMeService.DefaultEncryptionKey = "ek_test_DSO5EqJemg8fA6bFtDU5aSj4YA7tP0";
			PagarMeService.DefaultApiKey = "ak_test_RtRb4h796ZKnuyRF8ktDI4ovTjOkeu";
			//PagarMeService.DefaultEncryptionKey = "ek_test_UT6AN4fDN3BCUgo6kxUiOq6S20dbKc";
			//PagarMeService.DefaultApiKey = "ak_test_TSgC3nvXtdYnDoGKgNLIOfk3TFfkl9";
        }

        public async Task Initialize()
        {
            await _mpos.Initialize();

			Console.WriteLine ("Asking for tables to be synchronized...");
			await _mpos.SynchronizeTables(true);
			//Console.WriteLine ("SynchronizeTables called.");
			//_mpos.Display("Hello, world!");
        }

        public async Task Pay(int amount)
        {
			var result = await _mpos.ProcessPayment(amount, null, PagarMe.Mpos.PaymentMethod.Debit);
			Console.WriteLine ("CARD HASH = " + result.CardHash);

			await _mpos.Close ();
			Console.WriteLine ("CLOSED!");

            var transaction = new Transaction
                {
                    CardHash = result.CardHash,
                    Amount = amount,
                    ShouldCapture = false
                };

            await transaction.SaveAsync();
			Console.WriteLine ("TRANSACTION ID = " + transaction.Id);

			Console.WriteLine (transaction);
			Console.WriteLine ("Transaction ARC = " + transaction.AcquirerResponseCode + ", Id = " + transaction.Id);
			Console.WriteLine ("ACQUIRER RESPONSE CODE = " + transaction.AcquirerResponseCode);
			int x = Int32.Parse (transaction.AcquirerResponseCode);
			object obj = transaction["card_emv_response"];
			string response = obj == null ? null : obj.ToString (); 

			await _mpos.FinishTransaction(true, x, (string)obj);
			await _mpos.Close ();
        }
    }
}

