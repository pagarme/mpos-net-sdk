using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PagarMe.Bifrost.Commands;
using PagarMe.Bifrost.Providers;
using PagarMe.Mpos.Devices;
using PagarMe.Generic;

namespace PagarMe.Bifrost
{
    public class Context : IDisposable
    {
        private readonly MposBridge bridge;
        private readonly SemaphoreSlim locker;
        private IProvider provider;
        private IDevice device;

        private ContextStatus status;

        internal String DeviceId => device?.Id;

        public Context(MposBridge bridge, IProvider provider)
        {
            this.bridge = bridge;
            this.provider = provider;
            locker = new SemaphoreSlim(1, 1);
            status = ContextStatus.Uninitialized;
        }

        public Task<IDevice[]> ListDevices()
        {
            var devices = bridge.DeviceManager.FindAvailableDevices();
            return Task.FromResult(devices);
        }

        internal PaymentRequest.Type CurrentOperation { get; set; }

        public async Task<Boolean?> Initialize(InitializeRequest request, Action<Int32> onError)
        {
            await locker.WaitAsync();

            if (status == ContextStatus.Ready)
                return false;

            try
            {
                var device = bridge.DeviceManager.GetById(request.DeviceId);
                var dataPath = ensureDataPath(device, request.EncryptionKey);

                var completedInit = await provider.Open(new InitializationOptions
                {
                    Device = device,
                    EncryptionKey = request.EncryptionKey,
                    StoragePath = dataPath,
                    BaudRate = request.BaudRate,
                    OnError = onError
                }).SetTimeout(request.TimeoutMilliseconds);
                if (!completedInit) return null;

                if (!request.SimpleInitialize)
                {
                    await provider.SynchronizeTables(false);
                }

                this.device = device;
                status = ContextStatus.Ready;
            }
            finally
            {
                locker.Release(1);
            }

            return true;
        }

        public Task<StatusResponse> GetStatus()
        {
            var devices = bridge.DeviceManager.FindAvailableDevices();

            var response = new StatusResponse
            {
                Code = status,
                AvailableDevices = devices.Length
            };

            if (device != null)
            {
                response.ConnectedDeviceId = device.Id;
            }

            return Task.FromResult(response);
        }

        public async Task DisplayMessage(DisplayMessageRequest request)
        {
            await locker.WaitAsync();

            try
            {
                await provider.DisplayMessage(request?.Message ?? String.Empty);
            }
            finally
            {
                locker.Release(1);
            }
        }

        public async Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request)
        {
            await locker.WaitAsync();

            if (status != ContextStatus.Ready)
                throw new InvalidOperationException("Another operation is in progress");

            try
            {
                status = ContextStatus.InUse;

                return await provider.ProcessPayment(request);
            }
            finally
            {
                status = ContextStatus.Ready;
                locker.Release(1);
            }
        }

        public async Task FinishPayment(FinishPaymentRequest request)
        {
            await locker.WaitAsync();

            if (status != ContextStatus.Ready)
                throw new InvalidOperationException("Another operation is in progress");

            try
            {
                status = ContextStatus.InUse;

                await provider.FinishPayment(request);
            }
            finally
            {
                status = ContextStatus.Ready;
                locker.Release(1);
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

            var path = Path.Combine(bridge.Options.DataPath, BitConverter.ToString(hash)) + Path.DirectorySeparatorChar;

            Directory.CreateDirectory(path);

            return path;
        }

        public async Task Close()
        {
            try
            {
                await provider.Close();
            }
            catch
            {
                // Doesn't matter, we're resetting anyway
            }

            status = ContextStatus.Uninitialized;
        }

        public void Dispose()
        {
            provider.Dispose();
            provider = null;
        }

        internal Boolean IsInUse()
        {
            return CurrentOperation == PaymentRequest.Type.Process
                || CurrentOperation == PaymentRequest.Type.Initialize;
        }
    }
}