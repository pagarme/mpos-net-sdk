using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PagarMe.Bifrost.Commands;
using PagarMe.Bifrost.Providers;
using PagarMe.Mpos.Devices;

namespace PagarMe.Bifrost
{
    public class Context : IDisposable
    {
        private readonly MposBridge _bridge;
        private readonly SemaphoreSlim _lock;
        private IProvider _provider;
        private IDevice _device;

        private ContextStatus _status;

        internal String DeviceId => _device?.Id;

        public Context(MposBridge bridge, IProvider provider)
        {
            _bridge = bridge;
            _provider = provider;
            _lock = new SemaphoreSlim(1, 1);
            _status = ContextStatus.Uninitialized;
        }

        public Task<IDevice[]> ListDevices()
        {
            var devices = _bridge.DeviceManager.FindAvailableDevices();
            return Task.FromResult(devices);
        }

        private Boolean initialized = false;

        internal PaymentRequest.Type CurrentOperation { get; set; }

        public async Task Initialize(InitializeRequest request, Action<Int32> onError)
        {
            lock (this)
            {
                if (initialized)
                    return;

                initialized = true;
            }

            await _lock.WaitAsync();

            try
            {
                var device = _bridge.DeviceManager.GetById(request.DeviceId);
                var dataPath = ensureDataPath(device, request.EncryptionKey);

                await _provider.Open(new InitializationOptions
                {
                    Device = device,
                    EncryptionKey = request.EncryptionKey,
                    StoragePath = dataPath,
                    BaudRate = request.BaudRate,
                    OnError = onError
                });

                if (!request.SimpleInitialize)
                {
                    await _provider.SynchronizeTables(false);
                }

                _device = device;
                _status = ContextStatus.Ready;
            }
            finally
            {
                _lock.Release(1);
            }
        }

        public Task<StatusResponse> GetStatus()
        {
            var devices = _bridge.DeviceManager.FindAvailableDevices();

            var response = new StatusResponse
            {
                Status = _status,
                AvailableDevices = devices.Length
            };

            if (_device != null)
                response.ConnectedDeviceId = _device.Id;

            return Task.FromResult(response);
        }

        public async Task DisplayMessage(DisplayMessageRequest request)
        {
            await _lock.WaitAsync();

            try
            {
                await _provider.DisplayMessage(request?.Message ?? String.Empty);
            }
            finally
            {
                _lock.Release(1);
            }
        }

        public async Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request)
        {
            await _lock.WaitAsync();

            if (_status != ContextStatus.Ready)
                throw new InvalidOperationException("Another operation is in progress");

            try
            {
                _status = ContextStatus.InUse;

                return await _provider.ProcessPayment(request);
            }
            finally
            {
                _status = ContextStatus.Ready;
                _lock.Release(1);
            }
        }

        public async Task FinishPayment(FinishPaymentRequest request)
        {
            await _lock.WaitAsync();

            if (_status != ContextStatus.Ready)
                throw new InvalidOperationException("Another operation is in progress");

            try
            {
                _status = ContextStatus.InUse;

                await _provider.FinishPayment(request);
            }
            finally
            {
                _status = ContextStatus.Ready;
                _lock.Release(1);
            }
        }

        private string ensureDataPath(IDevice device, string encryptionKey)
        {
            var hashText = "";

            hashText += encryptionKey;
            hashText += device.Name;
            hashText += device.Manufacturer;

            var hashData = Encoding.UTF8.GetBytes(hashText);
            var hash = SHA256.Create().ComputeHash(hashData);

            var path = Path.Combine(_bridge.Options.DataPath, BitConverter.ToString(hash)) + Path.DirectorySeparatorChar;

            Directory.CreateDirectory(path);

            return path;
        }

        public async Task Close()
        {
            try
            {
                await _provider.Close();
            }
            catch
            {
                // Doesn't matter, we're resetting anyway
            }

            _status = ContextStatus.Uninitialized;
        }

        public void Dispose()
        {
            _provider.Dispose();
            _provider = null;
        }
    }
}