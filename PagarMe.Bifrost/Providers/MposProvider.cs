using System;
using System.Threading.Tasks;
using PagarMe.Bifrost.Commands;
using mpos = PagarMe.Mpos.Mpos;
using PagarMe.Mpos.Devices;
using PagarMe.Mpos;

namespace PagarMe.Bifrost.Providers
{
    public class MposProvider : IProvider
    {
        private mpos mpos;
        private IDevice device;
        private Action<Int32> onError;

        public Task Open(InitializationOptions options)
        {
            device = options.Device;
            var stream = device.Open(options.BaudRate);

            mpos = new mpos(stream, options.EncryptionKey, options.StoragePath);

            mpos.Errored += errored;

            onError = options.OnError;

            return Task.Run(mpos.Initialize);
        }

        private void errored(object sender, int error)
        {
            onError(error);
        }


        public Task SynchronizeTables(bool force)
        {
            return mpos.SynchronizeTables(force);
        }

        public Task DisplayMessage(string message)
        {
            return Task.Run(() => mpos.Display(message));
        }

        public async Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request)
        {
            var paymentMethod = request.MagstripePaymentMethod;

            if (paymentMethod == 0)
                paymentMethod = PaymentMethod.Credit;

            var response = await mpos.ProcessPayment(request.Amount, request.Applications,
                                                      paymentMethod);

            return new ProcessPaymentResponse
            {
                Result = response
            };
        }

        public Task FinishPayment(FinishPaymentRequest request)
        {
            return mpos.FinishTransaction(request.Success, request.ResponseCode, request.EmvData);
        }

        public Task CancelOperation()
        {
            mpos.Cancel();

            return Task.FromResult(0);
        }


        public async Task Close()
        {
            await mpos.Close();
        }

        public void Dispose()
        {
            mpos?.Dispose();
            mpos = null;

            device?.Close();
        }
    }
}