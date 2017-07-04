using System;
using System.IO.Ports;
using System.Threading.Tasks;
using PagarMe.Generic;

namespace PagarMe.Mpos.Example
{
    public class PaymentProcessor
    {
        private readonly Mpos mpos;
        private readonly SerialPort port;

        public PaymentProcessor(string device)
        {
            port = new SerialPort(device, Config.BaudRate, Parity.None, 8, StopBits.One);
            port.Open();

            mpos = new Mpos(port.BaseStream, Config.EncryptionKey, Config.SqlitePath);
            mpos.NotificationReceived += (sender, e) => Console.WriteLine("Status: {0}", e);
            mpos.TableUpdated += (sender, e) => Console.WriteLine("LOADED: {0}", e);
            mpos.Errored += (sender, e) => Console.WriteLine("I GOT ERROR {0}", e);
            mpos.PaymentProcessed += (sender, e) => Console.WriteLine("HEY CARD HASH " + e.CardHash);
            mpos.FinishedTransaction += (sender, e) => Console.WriteLine("FINISHED TRANSACTION!");

            PagarMeService.DefaultEncryptionKey = Config.EncryptionKey;
            PagarMeService.DefaultApiKey = Config.ApiKey;
        }

        public async Task Initialize()
        {
            await mpos.Initialize();

            Console.WriteLine("Asking for tables to be synchronized...");
            await mpos.SynchronizeTables(true);
            Console.WriteLine("SynchronizeTables called.");
        }

        public async Task Pay(int amount)
        {
            var result = await mpos.ProcessPayment(amount, null, PaymentMethod.Debit);
            Console.WriteLine("CARD HASH = " + result.CardHash);

            await mpos.Close();
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

            await mpos.FinishTransaction(true, x, (string) obj);
            await mpos.Close();
        }
    }
}
