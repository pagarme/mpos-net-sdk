using System;
using System.IO.Ports;
using System.Threading.Tasks;
using PagarMe.Generic;

namespace PagarMe.Mpos.Example
{
    public class PaymentProcessor
    {
        private readonly Mpos _mpos;
        private readonly SerialPort _port;

        public PaymentProcessor(string device)
        {
            _port = new SerialPort(device, Config.BaudRate, Parity.None, 8, StopBits.One);
            _port.Open();

            _mpos = new Mpos(_port.BaseStream, Config.EncryptionKey, Config.SqlitePath);
            _mpos.NotificationReceived += (sender, e) => Console.WriteLine("Status: {0}", e);
            _mpos.TableUpdated += (sender, e) => Console.WriteLine("LOADED: {0}", e);
            _mpos.Errored += (sender, e) => Console.WriteLine("I GOT ERROR {0}", e);
            _mpos.PaymentProcessed += (sender, e) => Console.WriteLine("HEY CARD HASH " + e.CardHash);
            _mpos.FinishedTransaction += (sender, e) => Console.WriteLine("FINISHED TRANSACTION!");

            PagarMeService.DefaultEncryptionKey = Config.EncryptionKey;
            PagarMeService.DefaultApiKey = Config.ApiKey;
        }

        public async Task Initialize()
        {
            await _mpos.Initialize();

            Console.WriteLine("Asking for tables to be synchronized...");
            await _mpos.SynchronizeTables(true);
            Console.WriteLine("SynchronizeTables called.");
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
