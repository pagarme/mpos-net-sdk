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
        private mpos _mpos;
        private IDevice device;
        private Action<Int32> onError;

        public Task Open(InitializationOptions options)
        {
            device = options.Device;
            var stream = device.Open(options.BaudRate);

            _mpos = new mpos(stream, options.EncryptionKey, options.StoragePath);

            _mpos.Errored += errored;

            onError = options.OnError;

            return Task.Run(_mpos.Initialize);
        }

        private void errored(object sender, int error)
        {
            onError(error);
        }


        public Task SynchronizeTables(bool force)
        {
            return _mpos.SynchronizeTables(force);
        }

        public Task DisplayMessage(string message)
        {
            return Task.Run(() => _mpos.Display(message));
        }

        public async Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request)
        {
            var paymentMethod = request.MagstripePaymentMethod;

            if (paymentMethod == 0)
                paymentMethod = PaymentMethod.Credit;

            var response = await _mpos.ProcessPayment(request.Amount, request.Applications,
                                                      paymentMethod);

            return new ProcessPaymentResponse
            {
                Result = response
            };
        }

        public Task FinishPayment(FinishPaymentRequest request)
        {
            return _mpos.FinishTransaction(request.Success, request.ResponseCode, request.EmvData);
        }

        public Task CancelOperation()
        {
            _mpos.Cancel();

            return Task.FromResult(0);
        }


        public async Task Close()
        {
            await _mpos.Close();
        }

        public void Dispose()
        {
            _mpos?.Dispose();
            _mpos = null;

            device?.Close();
        }
    }
}