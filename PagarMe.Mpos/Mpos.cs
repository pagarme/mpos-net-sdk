using System;
using System.IO;
using System.Collections.Generic;
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

        private static IntPtr GetMarshalBytes<T>(T str) {
            int size = Marshal.SizeOf(typeof(T));

            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, false);

            return ptr;
        }

        private AbecsStream _stream;
        private IntPtr _nativeMpos;
        private readonly string _encryptionKey;

        private Native.MposNotificationCallbackDelegate NotificationPin;
        private Native.MposOperationCompletedCallbackDelegate OperationPin;

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
            NotificationPin = HandleNotificationCallback;
            OperationPin = HandleOperationCompletedCallback;

            _stream = stream;
            _encryptionKey = encryptionKey;
            _nativeMpos = Native.Create((IntPtr)stream.NativeStream, NotificationPin, OperationPin);
        }

        ~Mpos()
        {
            Dispose(false);
        }

        public Task Initialize()
        {
            GCHandle pin = default(GCHandle);
            var source = new TaskCompletionSource<bool>();

            Native.MposInitializedCallbackDelegate callback = mpos =>
                {
                    pin.Free();

                    try
                    {
                        OnInitialized();

                        source.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        source.TrySetException(ex);
                    }

                    return Native.Error.Ok;
                };

            pin = GCHandle.Alloc(pin);

            Native.Error error = Native.Initialize(_nativeMpos, IntPtr.Zero, callback);

            if (error != Native.Error.Ok)
                throw new MposException(error);

            return source.Task;
        }

        public async Task SynchronizeTables()
        {
            GCHandle pin = default(GCHandle);
            var source = new TaskCompletionSource<bool>();

            CapkEntry[] capks = await ApiHelper.GetTerminalTable<CapkEntry>("capks");
            AidEntry[] aids = await ApiHelper.GetTerminalTable<AidEntry>("aids");

            Native.Capk[] nativeCapk = capks.Select(x => new Native.Capk(x)).ToArray();
            Native.Aid[] nativeAid = aids.Select(x => new Native.Aid(x)).ToArray();

            List<IntPtr> tables = new List<IntPtr>();

            foreach (var aid in nativeAid)
                tables.Add(GetMarshalBytes(aid));

            foreach (var capk in nativeCapk)
                tables.Add(GetMarshalBytes(capk));

            Native.MposFinishTransacitonCallbackDelegate callback = (mpos, err) =>
                {
                    pin.Free();

                    foreach (IntPtr ptr in tables)
                        Marshal.FreeHGlobal(ptr);

                    source.SetResult(true);

                    return Native.Error.Ok;
                };

            pin = GCHandle.Alloc(callback);

            Native.Error error = Native.UpdateTables(_nativeMpos, tables.ToArray(), tables.Count, callback);

            if (error != Native.Error.Ok)
                throw new MposException(error);

            await source.Task;
        }


        public Task<PaymentResult> ProcessPayment(int amount, PaymentFlags flags = PaymentFlags.Default)
        {
            GCHandle pin = default(GCHandle);
            var source = new TaskCompletionSource<PaymentResult>();

            Native.MposPaymentCallbackDelegate callback = (mpos, err, infoPointer) =>
            {
                var info = (Native.PaymentInfo)Marshal.PtrToStructure(infoPointer, typeof(Native.PaymentInfo));

                pin.Free();

                HandlePaymentCallback(err, info).ContinueWith(t =>
                    {
                        if (t.Status == TaskStatus.Faulted)
                        {
                            source.SetException(t.Exception);
                        }
                        else
                        {
                            source.SetResult(t.Result);
                        }

                        OnPaymentProcessed(t.Result);
                    });

                return Native.Error.Ok;
            };

            pin = GCHandle.Alloc(callback);

            Native.Error error = Native.ProcessPayment(_nativeMpos, amount, flags, callback);

            if (error != Native.Error.Ok)
                throw new MposException(error);
            return source.Task;
        }

        public Task FinishTransaction(int responseCode, string emvData)
        {
            GCHandle pin = default(GCHandle);
            var source = new TaskCompletionSource<bool>();
            Native.TransactionStatus status;

            if (responseCode < 1000)
            {
                status = responseCode == 0 ? Native.TransactionStatus.Ok : Native.TransactionStatus.NonZero;
            }
            else
            {
                status = Native.TransactionStatus.Error;
            }

            Native.MposFinishTransacitonCallbackDelegate callback = (mpos, err) =>
            {
                pin.Free();

                source.SetResult(true);

                return Native.Error.Ok;
            };

            pin = GCHandle.Alloc(callback);

            Native.Error error = Native.FinishTransaction(_nativeMpos, status, responseCode, emvData.Length, emvData, callback);

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
                PaymentMethod paymentMethod = (PaymentMethod)info.ApplicationType;
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

                await result.BuildAccepted(this.EncryptionKey, status, paymentMethod, pan, holderName, expirationDate, track2, emv, isOnlinePin, pin, pinKek);
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

            public enum TransactionStatus
            {
                Ok = 0,
                Error = 1,
                NonZero = 9
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public unsafe struct PaymentInfo
            {
                [MarshalAs(UnmanagedType.I4)]
                public Decision Decision;

                public int Amount;
                public int AcquirerIndex;
                public int RecordNumber;
                public int ApplicationType;

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

                [MarshalAs(UnmanagedType.I1)]
                public bool IsOnlinePin;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] Pin;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
                public byte[] PinKek;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public unsafe struct Capk
            {
                [MarshalAs(UnmanagedType.I1)]
                public bool IsAid;
                public int AcquirerNumber;
                public int RecordIndex;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
                public byte[] Rid;
                public int CapkIndex;
                public int ExponentLength;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
                public byte[] Exponent;
                public int ModulusLength;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 496)]
                public byte[] Modulus;

                [MarshalAs(UnmanagedType.I1)]
                public bool HasChecksum;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
                public byte[] Checksum;

                public Capk(CapkEntry e)
                {
                    IsAid = false;
                    AcquirerNumber = e.AcquirerNumber;
                    RecordIndex = e.RecordIndex;

                    Rid = GetBytes(e.Rid, 10);
                    CapkIndex = e.PublicKeyId;
                    Exponent = GetHexBytes(Convert.FromBase64String(e.Exponent), 6, out ExponentLength, false);
                    Modulus = GetHexBytes(Convert.FromBase64String(e.Modulus), 496, out ModulusLength, false);
                    HasChecksum = e.Checksum != null;

                    if (HasChecksum)
                        Checksum = GetHexBytes(Convert.FromBase64String(e.Exponent), 40, false);
                    else
                        Checksum = GetHexBytes("", 40);
                }
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public unsafe struct Aid
            {
                [MarshalAs(UnmanagedType.I1)]
                public bool IsAid;
                public int AcquirerNumber;
                public int RecordIndex;

                public int AidLength;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
                public byte[] AidNumber;
                public int ApplicationType;
                public int ApplicationNameLength;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] ApplicationName;
                public int CountryCode;
                public int Currency;
                public int CurrencyExponent;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
                public byte[] MerchantId;
                public int Mcc;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
                public byte[] TerminalCapabilities;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
                public byte[] AdditionalTerminalCapabilities;
                public int TerminalType;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
                public byte[] DefaultTac;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
                public byte[] DenialTac;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
                public byte[] OnlineTac;
                public int FloorLimit;
                [MarshalAs(UnmanagedType.I1)]
                public byte Tcc;

                [MarshalAs(UnmanagedType.I1)]
                public bool CtlsZeroAm;
                public int CtlsMode;
                public int CtlsTransactionLimit;
                public int CtlsFloorLimit;
                public int CtlsCvmLimit;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] CtlsApplicationVersion;

                public int TdolLength;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
                public byte[] Tdol;
                public int DdolLength;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
                public byte[] Ddol;

                public Aid(AidEntry e)
                {
                    IsAid = true;
                    AcquirerNumber = e.AcquirerNumber;
                    RecordIndex = e.RecordIndex;

                    AidNumber = GetHexBytes(e.Aid, 32, out AidLength, false);
                    ApplicationType = e.ApplicationType;
                    ApplicationName = GetBytes(e.ApplicationName, 16, out ApplicationNameLength);
                    CountryCode = e.CountryCode;
                    Currency = e.Currency;
                    CurrencyExponent = e.CurrencyExponent;
                    MerchantId = GetHexBytes("", 15);
                    Mcc = 4816;

                    TerminalCapabilities = GetHexBytes(e.TerminalCapabilities, 6);
                    AdditionalTerminalCapabilities = GetHexBytes(e.AdditionalTerminalCapabilities, 10);
                    TerminalType = e.TerminalType;
                    DefaultTac = GetHexBytes(e.DefaultTac, 10);
                    DenialTac = GetHexBytes(e.DenialTac, 10);
                    OnlineTac = GetHexBytes(e.OnlineTac, 10);
                    FloorLimit = e.FloorLimit;
                    Tcc = (byte)'T';

                    CtlsZeroAm = e.ContactlessZeroOnlineOnly;
                    CtlsMode = e.ContactlessMode;
                    CtlsTransactionLimit = e.ContactlessTransactionLimit;
                    CtlsFloorLimit = e.ContactlessFloorLimit;
                    CtlsCvmLimit = e.ContactlessCvmLimit;
                    CtlsApplicationVersion = GetHexBytes(e.ContactlessApplicationVersion.ToString("X4"), 4);

                    Tdol = GetBytes((e.Tdol.Length / 2).ToString("X2") + e.Tdol, 40, out TdolLength);
                    Ddol = GetBytes((e.Ddol.Length / 2).ToString("X2") + e.Ddol, 40, out DdolLength);
                }
            }

            public static byte[] GetBytes(string data, int length, out int newSize, char? fill = null, bool padLeft = true)
            {
                newSize = Encoding.UTF8.GetByteCount(data);

                if (fill.HasValue && data.Length < length)
                    data = padLeft ? data.PadLeft(length, fill.Value) : data.PadRight(length, fill.Value);

                byte[] result = Encoding.UTF8.GetBytes(data);
                byte[] full = new byte[length];

                Buffer.BlockCopy(result, 0, full, 0, result.Length);

                return full;
            }

            public static byte[] GetBytes(string data, int length, char? fill = null, bool padLeft = true)
            {
                int newSize;

                return GetBytes(data, length, out newSize, fill, padLeft);
            }

            public static byte[] GetHexBytes(string data, int length, out int byteLength, bool padLeft = true)
            {
                byte[] result = GetBytes(data, length, out byteLength, '0', padLeft);

                byteLength /= 2;

                return result;
            }

            public static byte[] GetHexBytes(string data, int length, bool padLeft = true)
            {
                int newSize;

                return GetHexBytes(data, length, out newSize, padLeft);
            }

            public static byte[] GetHexBytes(byte[] data, int length, out int byteLength, bool padLeft = true)
            {
                return GetHexBytes(data.Select(x => x.ToString("X2")).Aggregate((a, b) => a + b), length, out byteLength, padLeft);
            }

            public static byte[] GetHexBytes(byte[] data, int length, bool padLeft = true)
            {
                return GetHexBytes(data.Select(x => x.ToString("X2")).Aggregate((a, b) => a + b), length, padLeft);
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void MposNotificationCallbackDelegate(IntPtr mpos, string notification);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void MposOperationCompletedCallbackDelegate(IntPtr mpos);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposInitializedCallbackDelegate(IntPtr mpos);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposPaymentCallbackDelegate(IntPtr mpos, Error error, IntPtr info);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposTablesLoadedCallbackDelegate(IntPtr mpos, Error error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposFinishTransacitonCallbackDelegate(IntPtr mpos, Error error);

            [DllImport("mpos", EntryPoint = "mpos_new", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Create(IntPtr stream, MposNotificationCallbackDelegate notificationCallback, MposOperationCompletedCallbackDelegate operationCompletedCallback);

            [DllImport("mpos", EntryPoint = "mpos_initialize", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern Error Initialize(IntPtr mpos, IntPtr streamData, MposInitializedCallbackDelegate initializedCallback);

            [DllImport("mpos", EntryPoint = "mpos_process_payment", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern Error ProcessPayment(IntPtr mpos, int amount, PaymentFlags flags, MposPaymentCallbackDelegate paymentCallback);

            [DllImport("mpos", EntryPoint = "mpos_update_tables", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern Error UpdateTables(IntPtr mpos, IntPtr[] data, int count, MposFinishTransacitonCallbackDelegate callback);

            [DllImport("mpos", EntryPoint = "mpos_finish_transaction", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern Error FinishTransaction(IntPtr mpos, TransactionStatus status, int arc, int emvLen, string emv, MposFinishTransacitonCallbackDelegate callback);

            [DllImport("mpos", EntryPoint = "mpos_display", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern Error Display(IntPtr mpos, string text);

            [DllImport("mpos", EntryPoint = "mpos_close", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern Error Close(IntPtr mpos);

            [DllImport("mpos", EntryPoint = "mpos_free", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern Error Free(IntPtr mpos);

        }
    }
}

