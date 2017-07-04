using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace PagarMe.Mpos.Abecs
{
    internal unsafe class AbecsStream : IDisposable
    {
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

        private readonly CancellationTokenSource cancellationToken;
        private readonly Native.StreamCloseDelegate ClosePin;

        private readonly Native.StreamOpenDelegate OpenPin;
        private readonly Native.StreamWriteDelegate WritePin;
        private Boolean open;
        private Boolean available;

        public AbecsStream(Stream baseStream)
        {
            OpenPin = Open;
            WritePin = Write;
            ClosePin = Close;

            BaseStream = baseStream;
            cancellationToken = new CancellationTokenSource();
            NativeStream = Native.Allocate();
            NativeStream->Open = OpenPin;
            NativeStream->Write = WritePin;
            NativeStream->Close = ClosePin;

            available = true;
        }

        public Stream BaseStream { get; }
        public Native* NativeStream { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AbecsStream()
        {
            Dispose(false);
        }

        public Error Open(Native* stream)
        {
            if (open)
                return Error.OkError;

            BeginRead();

            return Error.Ok;
        }

        public Error Write(Native* stream, IntPtr data, int len)
        {
            var buffer = new byte[len];

            Marshal.Copy(data, buffer, 0, len);

            BaseStream.Write(buffer, 0, buffer.Length);

            return Error.Ok;
        }

        public Error Close(Native* stream)
        {
            if (!open)
                return Error.OkError;

            open = false;
            cancellationToken.Cancel();

            return Error.Ok;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (NativeStream != null)
            {
                Native.Free(NativeStream);
                NativeStream = null;
            }

            available = false;
        }

        private void BeginRead()
        {
            if (!available)
                return;

            var buffer = new byte[2048];

            BaseStream.ReadAsync(buffer, 0, buffer.Length).ContinueWith(t =>
            {
                NativeStream->DataReceived(NativeStream, buffer, t.Result);
                BeginRead();

            }, cancellationToken.Token);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Native
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error StreamOpenDelegate(Native* stream);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error StreamWriteDelegate(Native* stream, IntPtr buffer, int size);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error StreamCloseDelegate(Native* stream);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error StreamDataReceivedDelegate(Native* stream, byte[] buffer, int size);

            public static int Size
            {
                get { return Marshal.SizeOf(typeof(Native)); }
            }

            public IntPtr Abecs;

            public IntPtr OpenPointer;
            public IntPtr WritePointer;
            public IntPtr DataReceivedPointer;
            public IntPtr ClosePointer;

            public StreamOpenDelegate Open
            {
                get
                {
                    return
                        (StreamOpenDelegate)
                        Marshal.GetDelegateForFunctionPointer(OpenPointer, typeof(StreamOpenDelegate));
                }
                set { OpenPointer = Marshal.GetFunctionPointerForDelegate(value); }
            }

            public StreamWriteDelegate Write
            {
                get
                {
                    return
                        (StreamWriteDelegate)
                        Marshal.GetDelegateForFunctionPointer(WritePointer, typeof(StreamWriteDelegate));
                }
                set { WritePointer = Marshal.GetFunctionPointerForDelegate(value); }
            }

            public StreamCloseDelegate Close
            {
                get
                {
                    return
                        (StreamCloseDelegate)
                        Marshal.GetDelegateForFunctionPointer(ClosePointer, typeof(StreamCloseDelegate));
                }
                set { ClosePointer = Marshal.GetFunctionPointerForDelegate(value); }
            }

            public StreamDataReceivedDelegate DataReceived
            {
                get
                {
                    return
                        (StreamDataReceivedDelegate)
                        Marshal.GetDelegateForFunctionPointer(DataReceivedPointer, typeof(StreamDataReceivedDelegate));
                }
                set { DataReceivedPointer = Marshal.GetFunctionPointerForDelegate(value); }
            }

            public static Native* Allocate()
            {
                return (Native*) Marshal.AllocHGlobal(Size);
            }

            public static void Free(Native* stream)
            {
                Marshal.FreeHGlobal((IntPtr) stream);
            }
        }
    }
}