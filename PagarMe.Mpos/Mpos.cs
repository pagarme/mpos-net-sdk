using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PagarMe.Mpos
{
    public unsafe class Mpos : IDisposable
	{
        private AbecsStream _stream;
        private Native *_nativeMpos;
        private IntPtr _handlePaymentPointer;
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

		private Mpos(AbecsStream stream, string encryptionKey)
		{
			_stream = stream;
			_encryptionKey = encryptionKey;
            _nativeMpos = Native.Create(stream.NativeStream, HandleNotificationCallback, HandleOperationCompletedCallback);
            _handlePaymentPointer = Marshal.GetFunctionPointerForDelegate(new Native.MposPaymentCallbackDelegate(HandlePaymentCallback));
		}

        ~Mpos()
        {
            Dispose(false);
        }

        public void Initialize()
		{
            Error error = Native.Initialize(_nativeMpos, IntPtr.Zero, (mpos) => {
                OnInitialized();

                return Error.Ok;
            });

            if (error != Error.Ok)
                throw new MposException(error);
		}

        public void ProcessPayment(int amount, PaymentFlags flags = PaymentFlags.Default)
		{
            Error error = Native.ProcessPayment(_nativeMpos, amount, flags, _handlePaymentPointer);

            if (error != Error.Ok)
                throw new MposException(error);
		}

        public void Display(string text)
        {
            Error error = Native.Display(_nativeMpos, text);

            if (error != Error.Ok)
                throw new MposException(error);
        }

		public void Close()
		{
            Error error = Native.Close(_nativeMpos);

            if (error != Error.Ok)
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

            if (_nativeMpos != null)
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

        private unsafe Error HandlePaymentCallback(Native *mpos, Error error, ref Native.PaymentInfo info)
        {
            PaymentResult result = new PaymentResult();

            if (error == Error.Ok)
            {
                result.Status = PaymentStatus.Success;
                result.EmvData = Encoding.ASCII.GetString(info.EmvData, 0, info.EmvDataLength.ToInt32());
                result.Track2 = Encoding.ASCII.GetString(info.Track2, 0, info.Track2Length.ToInt32());
                result.Pan = Encoding.ASCII.GetString(info.Pan, 0, info.PanLength.ToInt32());
                result.ExpirationDate = Encoding.ASCII.GetString(info.ExpirationDate, 0, info.ExpirationDate.Length);

                result.CalculateCardHash(_encryptionKey);
            }
            else
            {
                result.Status = PaymentStatus.Error;
            }

            OnPaymentProcessed(result);


            return Error.Ok;
        }

        private unsafe void HandleNotificationCallback(Native *mpos, string notification)
        {
            if (NotificationReceived != null)
                NotificationReceived(this, notification);
        }

        private unsafe void HandleOperationCompletedCallback(Native *mpos)
        {
            if (OperationCompleted != null)
                OperationCompleted(this, new EventArgs());
        }

        internal enum Error
        {
            Ok,
            Error
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Native
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct PaymentInfo
            {
                public int Amount;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
                public byte[] ExpirationDate;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
                public byte[] Pan;
                public IntPtr PanLength;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
                public byte[] Track2;
                public IntPtr Track2Length;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
                public byte[] EmvData;
                public IntPtr EmvDataLength;
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void MposNotificationCallbackDelegate(Native *mpos, string notification);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void MposOperationCompletedCallbackDelegate(Native *mpos);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposInitializedCallbackDelegate(Native *mpos);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposPaymentCallbackDelegate(Native *mpos, Error error, ref PaymentInfo info);

            [DllImport("mpos", EntryPoint = "mpos_new", CharSet = CharSet.Ansi)]
            public static extern Native *Create(AbecsStream.Native*stream, MposNotificationCallbackDelegate notificationCallback, MposOperationCompletedCallbackDelegate operationCompletedCallback);

            [DllImport("mpos", EntryPoint = "mpos_initialize", CharSet = CharSet.Ansi)]
            public static extern Error Initialize(Native *mpos, IntPtr streamData, MposInitializedCallbackDelegate initializedCallback);

            [DllImport("mpos", EntryPoint = "mpos_process_payment", CharSet = CharSet.Ansi)]
            public static extern Error ProcessPayment(Native *mpos, int amount, PaymentFlags flags, IntPtr paymentCallback);

            [DllImport("mpos", EntryPoint = "mpos_display", CharSet = CharSet.Ansi)]
            public static extern Error Display(Native *mpos, string texxt);

            [DllImport("mpos", EntryPoint = "mpos_close", CharSet = CharSet.Ansi)]
            public static extern Error Close(Native *mpos);

            [DllImport("mpos", EntryPoint = "mpos_free", CharSet = CharSet.Ansi)]
            public static extern Error Free(Native *mpos);
        }
	}
}

