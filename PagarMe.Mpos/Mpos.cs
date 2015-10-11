using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Linq;

namespace PagarMe.Mpos
{
    public class Mpos : IDisposable
    {
        private static string GetString(byte[] data, IntPtr len)
        {
            return GetString(data, len.ToInt32());
        }

        private static string GetString(byte[] data, int len = -1)
        {
            if (len == -1)
                len = data.Length;

            return Encoding.ASCII.GetString(data, 0, len);
        }

        private AbecsStream _stream;
        private IntPtr _nativeMpos;
		private readonly string _encryptionKey;

		public event EventHandler Initialized;
        public event EventHandler<PaymentResult> PaymentProcessed;
        public event EventHandler<string> NotificationReceived;
        public event EventHandler OperationCompleted;

		public Stream BaseStream { get { return _stream.BaseStream; } }
		public string EncryptionKey { get { return _encryptionKey; } }

        public Mpos(Stream stream, string encryptionKey)
            : this(new AbecsStream(stream), encryptionKey)
        {
        }

		private unsafe Mpos(AbecsStream stream, string encryptionKey)
		{
			_stream = stream;
			_encryptionKey = encryptionKey;
            _nativeMpos = Native.Create((IntPtr)stream.NativeStream, HandleNotificationCallback, HandleOperationCompletedCallback);
		}

        ~Mpos()
        {
            Dispose(false);
        }

        public Task Initialize()
        {
            var source = new TaskCompletionSource<bool>();

            Native.Error error = Native.Initialize(_nativeMpos, IntPtr.Zero, mpos => {
                try {
                    OnInitialized();

                    source.TrySetResult(true);
                } catch(Exception ex) {
                    source.TrySetException(ex);
                }

                return Native.Error.Ok;
            });

            if (error != Native.Error.Ok)
                throw new MposException(error);

            return source.Task;
		}

        public Task<PaymentResult> ProcessPayment(int amount, PaymentFlags flags = PaymentFlags.Default)
		{
            var source = new TaskCompletionSource<PaymentResult>();

            Native.Error error = Native.ProcessPayment(_nativeMpos, amount, flags, (mpos, err, infoPointer) => {
                var info = Marshal.PtrToStructure<Native.PaymentInfo>(infoPointer);

                HandlePaymentCallback(err, info).ContinueWith(t => {
                    if (t.Status == TaskStatus.Faulted) {
                        source.SetException(t.Exception);
                    } else {
                        source.SetResult(t.Result);
                    }

                    OnPaymentProcessed(t.Result);
                });

                return Native.Error.Ok;
            });

            if (error != Native.Error.Ok)
                throw new MposException(error);

            return source.Task;
		}

        public void Display(string text)
        {
            Native.Error error = Native.Display(_nativeMpos, text);

            if (error != Native.Error.Ok)
                throw new MposException(error);
        }

		public void Close()
		{
            Native.Error error = Native.Close(_nativeMpos);

            if (error != Native.Error.Ok)
                throw new MposException(error);
		}
            
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }
            }

            if (_nativeMpos != IntPtr.Zero)
            {
                Native.Free(_nativeMpos);
            }
        }

        protected virtual void OnInitialized()
        {
            if (Initialized != null)
                Initialized(this, new EventArgs());
        }

        protected virtual void OnPaymentProcessed(PaymentResult result)
        {
            if (PaymentProcessed != null)
                PaymentProcessed(this, result);
        }

        private async Task<PaymentResult> HandlePaymentCallback(Native.Error error, Native.PaymentInfo info)
        {
            PaymentResult result = new PaymentResult();

            if (error == Native.Error.Ok)
            {
                PaymentStatus status = info.Decision == Native.Decision.Refused ? PaymentStatus.Rejected : PaymentStatus.Accepted;
                string emv = GetString(info.EmvData, info.EmvDataLength);
                string track2 = GetString(info.Track2, info.Track2Length);
                string pan = GetString(info.Pan, info.PanLength);
                string expirationDate = GetString(info.ExpirationDate);
                string holderName = GetString(info.HolderName);
                string pin = null, pinKek = null;
                bool isOnlinePin = info.IsOnlinePin;

                expirationDate = expirationDate.Substring(2, 2) + expirationDate.Substring(0, 2);
                holderName = holderName.Trim().Split('/').Reverse().Aggregate((a, b) => a + ' ' + b);

                if (isOnlinePin)
                {
                    pin = GetString(info.Pin);
                    pinKek = GetString(info.PinKek);
                }

                await result.BuildAccepted(this.EncryptionKey, status, PaymentMethod.Credit, pan, holderName, expirationDate, track2, emv, isOnlinePin, pin, pinKek);
            }
            else
            {
                result.BuildErrored();
            }

            return result;
        }

        private unsafe void HandleNotificationCallback(IntPtr mpos, string notification)
        {
            if (NotificationReceived != null)
                NotificationReceived(this, notification);
        }

        private unsafe void HandleOperationCompletedCallback(IntPtr mpos)
        {
            if (OperationCompleted != null)
                OperationCompleted(this, new EventArgs());
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Native
        {
            internal enum Error
            {
                Ok,
                Error
            }

            public enum Decision
            {
                Approved = 0,
                Refused,
                GoOnline
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public unsafe struct PaymentInfo
            {
                public Decision Decision;

                public int Amount;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
                public byte[] ExpirationDate;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
                public byte[] HolderName;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
                public byte[] Pan;
                public IntPtr PanLength;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 76)]
                public byte[] Track1;
                public IntPtr Track1Length;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
                public byte[] Track2;
                public IntPtr Track2Length;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 104)]
                public byte[] Track3;
                public IntPtr Track3Length;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
                public byte[] EmvData;
                public IntPtr EmvDataLength;

                [MarshalAs(UnmanagedType.Bool)]
                public bool IsOnlinePin;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] Pin;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
                public byte[] PinKek;
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void MposNotificationCallbackDelegate(IntPtr mpos, string notification);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void MposOperationCompletedCallbackDelegate(IntPtr mpos);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposInitializedCallbackDelegate(IntPtr mpos);


            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposPaymentCallbackDelegate(IntPtr mpos, Error error, IntPtr info);

            [DllImport("mpos", EntryPoint = "mpos_new", CharSet = CharSet.Ansi)]
            public static extern IntPtr Create(IntPtr stream, MposNotificationCallbackDelegate notificationCallback, MposOperationCompletedCallbackDelegate operationCompletedCallback);

            [DllImport("mpos", EntryPoint = "mpos_initialize", CharSet = CharSet.Ansi)]
            public static extern Error Initialize(IntPtr mpos, IntPtr streamData, MposInitializedCallbackDelegate initializedCallback);

            [DllImport("mpos", EntryPoint = "mpos_process_payment", CharSet = CharSet.Ansi)]
            public static extern Error ProcessPayment(IntPtr mpos, int amount, PaymentFlags flags, MposPaymentCallbackDelegate paymentCallback);

            [DllImport("mpos", EntryPoint = "mpos_display", CharSet = CharSet.Ansi)]
            public static extern Error Display(IntPtr mpos, string texxt);

            [DllImport("mpos", EntryPoint = "mpos_close", CharSet = CharSet.Ansi)]
            public static extern Error Close(IntPtr mpos);

            [DllImport("mpos", EntryPoint = "mpos_free", CharSet = CharSet.Ansi)]
            public static extern Error Free(IntPtr mpos);
        }
	}
}

