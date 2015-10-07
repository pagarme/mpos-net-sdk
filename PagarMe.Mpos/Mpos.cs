using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PagarMe.Mpos
{
    public unsafe class Mpos : IDisposable
	{
        private AbecsStream _stream;
        private Native *_nativeMpos;
		private readonly string _encryptionKey;

		public event EventHandler Initialized;
		public event EventHandler<PaymentResult> PaymentProcessed;
        public event UnhandledExceptionEventHandler Errored;

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
            _nativeMpos = Native.Create(stream.NativeStream);
            _nativeMpos->PaymentCallback = HandlePaymentCallback;
		}

        ~Mpos()
        {
            Dispose(false);
        }

        public void Initialize()
		{
            Error error = Native.Initialize(_nativeMpos, IntPtr.Zero);

            if (error != Error.Ok)
                throw new MposException(error);
		}

        public void ProcessPayment(int amount, PaymentFlags flags = PaymentFlags.Default)
		{
            Error error = Native.ProcessPayment(_nativeMpos, amount, flags);

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

        protected virtual void OnError(MposException exception)
        {
            if (Errored != null)
                Errored(this, new UnhandledExceptionEventArgs(exception, false));
        }

        protected virtual void OnPaymentProcessed(PaymentResult result)
        {
            if (PaymentProcessed != null)
                PaymentProcessed(this, result);
        }

        private unsafe Error HandlePaymentCallback(Native *mpos, Error error, IntPtr data, int dataLength)
        {
            if (error == Error.Ok)
            {
                byte[] buffer = new byte[dataLength];

                Marshal.Copy(data, buffer, 0, buffer.Length);

                OnPaymentProcessed(PaymentResult.FromEmvData(buffer, _encryptionKey));
            }
            else
            {
                OnError(new MposException(error));
            }

            return Error.Ok;
        }

        internal enum Error
        {
            Ok,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Native
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposPaymentCallbackDelegate(Native *mpos, Error error, IntPtr data, int dataLength);

            [DllImport("mpos", EntryPoint = "mpos_create")]
            public static extern Native *Create(AbecsStream.Native *stream);

            [DllImport("mpos", EntryPoint = "mpos_initialize")]
            public static extern Error Initialize(Native *mpos, IntPtr data);

            [DllImport("mpos", EntryPoint = "mpos_process_payment")]
            public static extern Error ProcessPayment(Native *mpos, int amount, PaymentFlags flags);

            [DllImport("mpos", EntryPoint = "mpos_close")]
            public static extern Error Close(Native *mpos);

            [DllImport("mpos", EntryPoint = "mpos_free")]
            public static extern Error Free(Native *mpos);

            public IntPtr Abecs;
            public IntPtr UserInfo;
            public IntPtr PaymentCallbackPointer;
       
            public MposPaymentCallbackDelegate PaymentCallback
            {
                get { return Marshal.GetDelegateForFunctionPointer<MposPaymentCallbackDelegate>(PaymentCallbackPointer); }
                set { PaymentCallbackPointer = Marshal.GetFunctionPointerForDelegate(value); }
            }
        }
	}
}

