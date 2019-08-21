using System;
using System.IO.Ports;
using System.Threading.Tasks;
using PagarMe.Mpos.Entities;

namespace PagarMe.Mpos.Example
{
    class PaymentProcessor : IDisposable
    {
        private readonly Mpos mpos;
        private readonly SerialPort port;

        public PaymentProcessor(String device)
        {
            port = new SerialPort(device, Config.BaudRate, Parity.None, 8, StopBits.One);
            port.Open();

            mpos = new Mpos(port.BaseStream, Config.EncryptionKey, Config.SqlitePath);

            mpos.NotificationReceived +=
                (sender, e) => PgDebugLog.Write($"Mpos Notification: {e}");

            mpos.TableUpdated +=
                (sender, e) => PgDebugLog.Write($"Table loaded: {e}");

            mpos.Errored +=
                (sender, e) => PgDebugLog.Write($"Error: {e}");

            mpos.PaymentProcessed +=
                (sender, e) => PgDebugLog.Write($"Payment processed for card hash: {e.CardHash}");

            mpos.FinishedTransaction +=
                (sender, e) => PgDebugLog.Write("Finished");

            PagarMeService.DefaultEncryptionKey = Config.EncryptionKey;
            PagarMeService.DefaultApiKey = Config.ApiKey;
        }

        public async Task Initialize()
        {
            await mpos.Initialize();

            PgDebugLog.Write("SynchronizeTables will be called");
            await mpos.SynchronizeTables(true);
            PgDebugLog.Write("SynchronizeTables called");
        }

        public async Task Pay(int amount)
        {
            PgDebugLog.Write("Starting payment");
            var result = await mpos.ProcessPayment(amount, null, Entities.PaymentMethod.Debit);
            PgDebugLog.Write("Payment start called");
            PgDebugLog.Write("Card hash: " + result.CardHash);

            if (result.Status == PaymentStatus.Accepted)
            {
                var transaction = new Transaction
                {
                    CardHash = result.CardHash,
                    Amount = amount,
                    ShouldCapture = false
                };

                PgDebugLog.Write("Sending transaction");
                await transaction.SaveAsync();
                PgDebugLog.Write("Transaction ID " + transaction.Id);
                PgDebugLog.Write("Acquirer response code = " + transaction.AcquirerResponseCode);

                var responseCode = Int32.Parse(transaction.AcquirerResponseCode);
                var emvResponse = transaction["card_emv_response"];
                var response = emvResponse?.ToString();

                if (result.IsOnlinePin)
                {
                    if (responseCode == 0)
                    {
                        PgDebugLog.Write("Calling success finish");
                        var finishedOk = await mpos.FinishTransaction(true, responseCode, response);

                        if (finishedOk)
                        {
                            PgDebugLog.Write("Success finish called");
                        }
                        else
                        {
                            PgDebugLog.Write("Card rejected finish");
                            transaction.Refund();
                        }
                    }
                    else
                    {
                        await finishCancelling(responseCode, response);
                    }
                }
            }
            else if (result.Status != PaymentStatus.Canceled && result.IsOnlinePin)
            {
                await finishCancelling();
            }

            PgDebugLog.Write("Will close");
            await mpos.Close();
            PgDebugLog.Write("Closed");
        }

        private async Task finishCancelling(Int32 responseCode = 0, String response = null)
        {
            PgDebugLog.Write("Calling error finish");
            // if finish rejects the transaction
            // there is no problem,
            // because API already rejected it
            await mpos.FinishTransaction(false, responseCode, response);
            PgDebugLog.Write("Error finish called");
        }

        public void Dispose()
        {
            port?.Close();
        }
    }
}
