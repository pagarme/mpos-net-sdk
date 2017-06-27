using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PagarMe.Mpos.Abecs;
using PagarMe.Mpos.Callbacks;

namespace PagarMe.Mpos
{
    public partial class Mpos : IDisposable
    {
        protected internal readonly IntPtr _nativeMpos;

        private AbecsStream _stream;

        private readonly Native.MposNotificationCallbackDelegate NotificationPin;
        private readonly Native.MposOperationCompletedCallbackDelegate OperationPin;

        public Mpos(Stream stream, string encryptionKey, string storagePath)
            : this(new AbecsStream(stream), encryptionKey, storagePath) { }

        private unsafe Mpos(AbecsStream stream, string encryptionKey, string storagePath)
        {
            NotificationPin = HandleNotificationCallback;
            OperationPin = HandleOperationCompletedCallback;

            _stream = stream;
            EncryptionKey = encryptionKey;
            StoragePath = storagePath;
            _nativeMpos = Native.Create(stream, NotificationPin, OperationPin);
            TMSStorage = new TMSStorage(storagePath, encryptionKey);
        }

        public Stream BaseStream
        {
            get { return _stream.BaseStream; }
        }

        public string EncryptionKey { get; }
        public string StoragePath { get; }
        public TMSStorage TMSStorage { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected internal static string GetString(byte[] data, IntPtr len)
        {
            return GetString(data, len.ToInt32());
        }

        protected internal static string GetString(byte[] data, int len = -1)
        {
            if (len == -1)
                len = data.Length;

            return Encoding.ASCII.GetString(data, 0, len);
        }

        public event EventHandler<int> Errored;
        public event EventHandler Initialized;
        public event EventHandler Closed;
        public event EventHandler<PaymentResult> PaymentProcessed;
        public event EventHandler<bool> TableUpdated;
        public event EventHandler FinishedTransaction;
        public event EventHandler<string> NotificationReceived;
        public event EventHandler OperationCompleted;

        ~Mpos()
        {
            Dispose(false);
        }

        public Task Initialize()
        {
            var source = new TaskCompletionSource<bool>();

            var callback = MposInitializedCallback.Callback(this, source);

            var error = Native.Initialize(_nativeMpos, callback);

            if (error != Native.Error.Ok)
                throw new MposException(error);

            return source.Task;
        }



        public Task SynchronizeTables(bool forceUpdate)
        {
            var source = new TaskCompletionSource<bool>();
            var keysCallback = MposExtractKeysCallback.Callback(this, forceUpdate, source);

            var keysError = Native.ExtractKeys(_nativeMpos, keysCallback);
            if (keysError != Native.Error.Ok) throw new MposException(keysError);

            return source.Task;
        }


        public Task<PaymentResult> ProcessPayment(int amount, IEnumerable<EmvApplication> applications = null,
            PaymentMethod magstripePaymentMethod = PaymentMethod.Credit)
        {
            var source = new TaskCompletionSource<PaymentResult>();
            var tableCallback = MposTablesLoadedPaymentCallback.Callback(this, amount, applications, magstripePaymentMethod, source);
            var versionCallback = MposGetTableVersionCallback.Callback(this, tableCallback, amount, magstripePaymentMethod, source);

            var tableVersionError = Native.GetTableVersion(_nativeMpos, versionCallback);
            if (tableVersionError != Native.Error.Ok)
                throw new MposException(tableVersionError);

            return source.Task;
        }

        public Task FinishTransaction(bool success, int responseCode, string emvData)
        {
            var source = new TaskCompletionSource<bool>();

            Native.TransactionStatus status;
            int length;

            if (!success)
            {
                status = Native.TransactionStatus.Error;
                emvData = "";
                length = 0;
                responseCode = 0;
            }
            else
            {
                length = emvData == null ? 0 : emvData.Length;

                if (responseCode < 1000)
                    status = responseCode == 0 ? Native.TransactionStatus.Ok : Native.TransactionStatus.NonZero;
                else
                    status = Native.TransactionStatus.Error;
            }

            var callback = MposFinishTransactionCallback.Callback(this, source);

            var error = Native.FinishTransaction(_nativeMpos, status, responseCode, emvData, length, callback);

            if (error != Native.Error.Ok)
                throw new MposException(error);

            return source.Task;
        }


        public void Display(string text)
        {
            var error = Native.Display(_nativeMpos, text);

            if (error != Native.Error.Ok)
                throw new MposException(error);
        }

        public void Cancel()
        {
            Native.Cancel(_nativeMpos);
        }

        public Task Close()
        {
            var source = new TaskCompletionSource<bool>();
            var callback = MposClosedCallback.Callback(this, source);

            var error = Native.Close(_nativeMpos, "", callback);

            if (error != Native.Error.Ok)
                throw new MposException(error);

            return source.Task;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }

            if (_nativeMpos != IntPtr.Zero)
                Native.Free(_nativeMpos);
        }

        internal protected virtual void OnInitialized(int error)
        {
            if (error != 0)
                Errored(this, error);
            else if (Initialized != null)
                Initialized(this, new EventArgs());
        }

        protected internal virtual void OnClosed(int error)
        {
            if (error != 0)
                Errored(this, error);
            else if (Closed != null)
                Closed(this, new EventArgs());
        }

        internal virtual void OnPaymentProcessed(PaymentResult result, int error)
        {
            if (error != 0)
                Errored(this, error);
            else if (PaymentProcessed != null)
                PaymentProcessed(this, result);
        }


        internal virtual void OnTableUpdated(bool loaded, int error)
        {
            if (error != 0)
                Errored(this, error);
            else if (TableUpdated != null)
                TableUpdated(this, loaded);
        }

        protected internal virtual void OnFinishedTransaction(int error)
        {
            if (error != 0)
                Errored(this, error);
            else if (FinishedTransaction != null)
                FinishedTransaction(this, new EventArgs());
        }

        internal async Task<PaymentResult> HandlePaymentCallback(int error, Native.PaymentInfo info)
        {
            var result = new PaymentResult();

            if (error == 0)
            {
                result.Fill(info);
                await result.BuildAccepted(EncryptionKey);
            }
            else
            {
                result.BuildErrored();
            }

            return result;
        }


        private void HandleNotificationCallback(IntPtr mpos, string notification)
        {
            if (NotificationReceived != null)
                NotificationReceived(this, notification);
        }

        private void HandleOperationCompletedCallback(IntPtr mpos)
        {
            if (OperationCompleted != null)
                OperationCompleted(this, new EventArgs());
        }
    }
}
