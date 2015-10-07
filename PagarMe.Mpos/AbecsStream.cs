using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PagarMe.Mpos
{
    internal unsafe class AbecsStream : IDisposable
    {
        private bool _open;
        private Native *_nativeStream;
        private readonly CancellationTokenSource _cancellationToken;
        private readonly Stream _baseStream;

        public Stream BaseStream { get { return _baseStream; } }
        public Native *NativeStream { get { return _nativeStream; } }

        public AbecsStream(Stream baseStream)
        {
            _baseStream = baseStream;
            _cancellationToken = new CancellationTokenSource();
            _nativeStream = Native.Allocate();
            _nativeStream->Open = Open;
            _nativeStream->Write = Write;
            _nativeStream->Close = Close;
        }

        ~AbecsStream()
        {
            Dispose(false);
        }

        public unsafe Error Open(Native *stream)
        {
            if (_open)
                return Error.OkError;

            BeginRead();

            return Error.Ok;
        }

        public unsafe Error Write(Native *stream, IntPtr data, int len)
        {
            byte[] buffer = new byte[len];

            Marshal.Copy(data, buffer, 0, len);

            _baseStream.Write(buffer, 0, buffer.Length);

            return Error.Ok;
        }

        public unsafe Error Close(Native *stream)
        {
            if (!_open)
                return Error.OkError;

            _open = false;
            _cancellationToken.Cancel();         
           
            return Error.Ok;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_nativeStream != null)
            {
                Native.Free(_nativeStream);
                _nativeStream = null;
            }
        }

        private void BeginRead()
        {
            byte[] buffer = new byte[4096];

            _baseStream.ReadAsync(buffer, 0, buffer.Length).ContinueWith(t =>
                {
                    _nativeStream->DataReceived(_nativeStream, buffer, t.Result);

                    BeginRead();
                }, _cancellationToken.Token);
        }

        public enum Error
        {
            Ok = 0,
            OkNtm = 1,
            OkError = 2,
            OkCompleted = 3,
            CommError = 10,
            BadCrcSent = 11,
            BadCrcReceived = 12,
            BadLengthReceived = 13,
            Timeout = 14,
            NoCommand = 15,
            FileError = 20,
            InternalError = 21

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Native
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error StreamOpenDelegate(Native *stream);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error StreamWriteDelegate(Native *stream, IntPtr buffer, int size);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error StreamCloseDelegate(Native *stream);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error StreamDataReceivedDelegate(Native *stream, byte[] buffer, int size);

            public static int Size { get { return Marshal.SizeOf<Native>(); } }

            public IntPtr OpenPointer;
            public IntPtr WritePointer;
            public IntPtr ClosePointer;
            public IntPtr DataReceivedPointer;
            public IntPtr UserData;

            public StreamOpenDelegate Open
            {
                get { return Marshal.GetDelegateForFunctionPointer<StreamOpenDelegate>(OpenPointer); }
                set { OpenPointer = Marshal.GetFunctionPointerForDelegate(value); }
            }

            public StreamWriteDelegate Write
            {
                get { return Marshal.GetDelegateForFunctionPointer<StreamWriteDelegate>(WritePointer); }
                set { WritePointer = Marshal.GetFunctionPointerForDelegate(value); }
            }
                
            public StreamCloseDelegate Close
            {
                get { return Marshal.GetDelegateForFunctionPointer<StreamCloseDelegate>(ClosePointer); }
                set { ClosePointer = Marshal.GetFunctionPointerForDelegate(value); }
            }

            public StreamDataReceivedDelegate DataReceived
            {
                get { return Marshal.GetDelegateForFunctionPointer<StreamDataReceivedDelegate>(OpenPointer); }
                set { OpenPointer = Marshal.GetFunctionPointerForDelegate(value); }
            }
           
            public static unsafe Native *Allocate()
            {
                return (Native *)Marshal.AllocHGlobal(Size);
            }

            public static unsafe void Free(Native *stream)
            {
                Marshal.FreeHGlobal((IntPtr)stream);
            }
        }
    }
}

