using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PagarMe.Mpos.Abecs;
using PagarMe.Mpos.Callbacks;
using PagarMe.Mpos.Entities;
using PagarMe.Mpos.Natives;
using PagarMe.Mpos.Tms;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos
{
    public class Mpos : IDisposable
    {
        protected internal readonly IntPtr nativeMpos;

        private AbecsStream stream;

        private readonly MposNotificationCallbackDelegate NotificationPin;
        private readonly MposOperationCompletedCallbackDelegate OperationPin;

        public Mpos(Stream stream, string encryptionKey, string storagePath)
            : this(new AbecsStream(stream), encryptionKey, storagePath) { }

        private Mpos(AbecsStream stream, string encryptionKey, string storagePath)
        {
            NotificationPin = HandleNotificationCallback;
            OperationPin = HandleOperationCompletedCallback;

            this.stream = stream;
            EncryptionKey = encryptionKey;
            StoragePath = storagePath;
            nativeMpos = Create(stream, NotificationPin, OperationPin);
            TMSStorage = new TMSStorage(storagePath);
        }

        public Stream BaseStream => stream.BaseStream;

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

            PgDebugLog.WriteLocal("Start Initialize");
            var error = Native.Initialize(nativeMpos, callback);
            PgDebugLog.WriteLocal($"Called Initialize: result code [{error}]");

            if (error != Error.Ok)
                throw new MposException(error);

            return source.Task;
        }



        public Task SynchronizeTables(bool forceUpdate)
        {
            var source = new TaskCompletionSource<bool>();
            var keysCallback = MposExtractKeysCallback.Callback(this, forceUpdate, source);

            var keysError = ExtractKeys(nativeMpos, keysCallback);
            if (keysError != Error.Ok) throw new MposException(keysError);

            return source.Task;
        }


        public Task<PaymentResult> ProcessPayment(int amount, IEnumerable<EmvApplication> applications = null,
            PaymentMethod magstripePaymentMethod = PaymentMethod.Credit, bool contactlessDisabled = false)
        {
            var source = new TaskCompletionSource<PaymentResult>();
            var tableCallback = MposTablesLoadedPaymentCallback.Callback(this, amount, applications, magstripePaymentMethod, contactlessDisabled, source);
            var versionCallback = MposGetTableVersionCallback.Callback(this, tableCallback, amount, magstripePaymentMethod, source);

            var tableVersionError = GetTableVersion(nativeMpos, versionCallback);
            if (tableVersionError != Error.Ok)
                throw new MposException(tableVersionError);

            return source.Task;
        }

        public Task FinishTransaction(bool success, int responseCode, string emvData)
        {
            var source = new TaskCompletionSource<bool>();

            TransactionStatus status;
            int length;

            if (!success)
            {
                status = TransactionStatus.Error;
                emvData = "";
                length = 0;
                responseCode = 0;
            }
            else
            {
                length = emvData == null ? 0 : emvData.Length;

                if (responseCode < 1000)
                    status = responseCode == 0 ? TransactionStatus.Ok : TransactionStatus.NonZero;
                else
                    status = TransactionStatus.Error;
            }

            var callback = MposFinishTransactionCallback.Callback(this, source);

            PgDebugLog.WriteLocal("Start FinishTransaction");
            var error = Native.FinishTransaction(nativeMpos, status, responseCode, emvData, length, callback);
            PgDebugLog.WriteLocal($"Called FinishTransaction: result code [{error}]");

            if (error != Error.Ok)
                throw new MposException(error);

            return source.Task;
        }


        public void Display(string text)
        {
            PgDebugLog.WriteLocal("Start Display");
            var error = Native.Display(nativeMpos, text);
            PgDebugLog.WriteLocal($"Called Display: result code [{error}]");

            if (error != Error.Ok)
                throw new MposException(error);
        }

        public void Cancel()
        {
            PgDebugLog.WriteLocal("Start Cancel");
            var error = Native.Cancel(nativeMpos);
            PgDebugLog.WriteLocal($"Called Cancel: result code [{error}]");
        }

        public Task Close()
        {
            var source = new TaskCompletionSource<bool>();
            var callback = MposClosedCallback.Callback(this, source);

            PgDebugLog.WriteLocal("Start Close");
            var error = Native.Close(nativeMpos, "", callback);
            PgDebugLog.WriteLocal($"Called Close: result code [{error}]");

            if (error != Error.Ok)
                throw new MposException(error);

            return source.Task;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }

            if (nativeMpos != IntPtr.Zero)
                Free(nativeMpos);
        }

        protected internal virtual void OnInitialized(int error)
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

        internal async Task<PaymentResult> HandlePaymentCallback(int error, PaymentInfo info)
        {
            var result = new PaymentResult();

            if (error == 0)
            {
                result.Fill(info);
                await result.BuildAccepted(EncryptionKey);
            }
            else
            {
                result.BuildErrored(error);
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
