using PagarMe.Generic;
using PagarMe.Mpos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using mpos = PagarMe.Mpos.Mpos;
using paymentMethod = PagarMe.Mpos.PaymentMethod;

namespace PagarMe.AutomatedTests
{
    public class PaymentProcessor
    {
        private readonly mpos _mpos;

        public IList<String> FinalResult { get; set; }

        public PaymentProcessor(Stream stream)
        {
            FinalResult = new List<String>();

            _mpos = new mpos(stream, Config.EncryptionKey, Config.SqlitePath);
            _mpos.NotificationReceived += notificationReceived;
            _mpos.TableUpdated += tableUpdated;
            _mpos.Errored += errored;
            _mpos.PaymentProcessed += paymentProcessed;
            _mpos.FinishedTransaction += finishedTransaction;
            _mpos.Closed += closed;
            _mpos.Initialized += initialized;

            PagarMeService.DefaultEncryptionKey = Config.EncryptionKey;
            PagarMeService.DefaultApiKey = Config.ApiKey;
        }



        private void notificationReceived(object sender, String e)
        {
            FinalResult.Add($"Notification Received: {e}");
        }

        private void tableUpdated(object sender, Boolean e)
        {
            FinalResult.Add($"Table Updated: {e}");
        }

        private void errored(object sender, Int32 e)
        {
            FinalResult.Add($"Errored: {e}");
        }

        private void paymentProcessed(object sender, PaymentResult e)
        {
            FinalResult.Add($"Payment Processed: {e.Status}");
        }

        private void finishedTransaction(object sender, EventArgs e)
        {
            FinalResult.Add($"Finished Transaction: {e}");
        }

        private void closed(object sender, EventArgs e)
        {
            FinalResult.Add($"Closed: {e}");
        }

        private void initialized(object sender, EventArgs e)
        {
            FinalResult.Add($"Initialized: {e}");
        }
        


        public async Task Pay(paymentMethod paymentMethod, int amount)
        {
            await _mpos.Initialize();

            FinalResult.Add("Ask Table Syncronization");
            await _mpos.SynchronizeTables(true);
            FinalResult.Add("Synchronize Tables called");
            //_mpos.Display("Hello, world!");

            var result = await _mpos.ProcessPayment(amount, magstripePaymentMethod: paymentMethod);

            FinalResult.Add($"Transaction Status: {result.Status}");

            await _mpos.Close();

            var transaction = new Transaction
            {
                CardHash = result.CardHash,
                Amount = amount,
                ShouldCapture = false
            };

            transaction.AcquirerResponseCode = "0000";
            transaction["card_emv_response"] = "000000000.00000";

            FinalResult.Add($"Transaction Saved: {transaction != null}");

            var acquirerResponseCode = int.Parse(transaction.AcquirerResponseCode);
            var cardEmvResponse = transaction["card_emv_response"];

            await _mpos.FinishTransaction(true, acquirerResponseCode, (string)cardEmvResponse);
            await _mpos.Close();

        }
    }
}