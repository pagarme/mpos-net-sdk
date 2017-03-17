using System;
using System.IO.Ports;
using System.Threading.Tasks;
using PagarMe;
using PagarMe.Mpos;
using PaymentMethod = PagarMe.Mpos.PaymentMethod;

namespace PaymentTest
{
    public class PaymentProcessor
    {
        private readonly Mpos _mpos;
        private readonly SerialPort _port;

        public PaymentProcessor(string device)
        {
            _port = new SerialPort(device, Config.BaudRate, Parity.None, 8, StopBits.One);
            _port.Open();

            _mpos = new Mpos(_port.BaseStream, "ek_test_f9cws0bU9700VqWE4UDuBlKLbvX4IO", Config.SqlitePath);
            //_mpos = new Mpos(_port.BaseStream, "ek_test_UT6AN4fDN3BCUgo6kxUiOq6S20dbKc");
            _mpos.NotificationReceived += (sender, e) => Console.WriteLine("Status: {0}", e);
            _mpos.TableUpdated += (sender, e) => Console.WriteLine("LOADED: {0}", e);
            _mpos.Errored += (sender, e) => Console.WriteLine("I GOT ERROR {0}", e);
            _mpos.PaymentProcessed += (sender, e) => Console.WriteLine("HEY CARD HASH " + e.CardHash);
            _mpos.FinishedTransaction += (sender, e) => Console.WriteLine("FINISHED TRANSACTION!");

            //PagarMeService.DefaultApiEndpoint = "http://192.168.64.2:3000";
            //PagarMeService.DefaultEncryptionKey = "ek_live_bspDfnKtdZahowfxSxuYTdYxaaDp1v";
            //PagarMeService.DefaultApiKey = "ak_live_JPHX33BR4omHj3ewCEghXsh12BH8VG";
            PagarMeService.DefaultEncryptionKey = "ek_test_f9cws0bU9700VqWE4UDuBlKLbvX4IO";
            PagarMeService.DefaultApiKey = "ak_test_NQEfPH4ktp7c9Zb0bpi1u1XkjpFCTH";
        }

        public async Task Initialize()
        {
            await _mpos.Initialize();

            Console.WriteLine("Asking for tables to be synchronized...");
            //await _mpos.SynchronizeTables(true);
            //Console.WriteLine ("SynchronizeTables called.");
            //_mpos.Display("Hello, world!");
        }

        public async Task Pay(int amount)
        {
            var result = await _mpos.ProcessPayment(amount, null, PaymentMethod.Debit);
            Console.WriteLine("CARD HASH = " + result.CardHash);

            await _mpos.Close();
            Console.WriteLine("CLOSED!");

            var transaction = new Transaction
            {
                CardHash = result.CardHash,
                Amount = amount,
                ShouldCapture = false
            };

            await transaction.SaveAsync();
            Console.WriteLine("TRANSACTION ID = " + transaction.Id);

            Console.WriteLine(transaction);
            Console.WriteLine("Transaction ARC = " + transaction.AcquirerResponseCode + ", Id = " + transaction.Id);
            Console.WriteLine("ACQUIRER RESPONSE CODE = " + transaction.AcquirerResponseCode);
            var x = int.Parse(transaction.AcquirerResponseCode);
            var obj = transaction["card_emv_response"];
            var response = obj == null ? null : obj.ToString();

            await _mpos.FinishTransaction(true, x, (string) obj);
            await _mpos.Close();
        }
    }
}