using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using PagarMe.Mpos.Abecs;
using PagarMe.Mpos.Entities;

namespace PagarMe.Mpos.Natives
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Native
    {
        public const Int32 ST_CANCEL = 13;

        public static byte[] GetBytes(string data, int length, out int newSize, char? fill = null,
            bool padLeft = true)
        {
            newSize = Encoding.UTF8.GetByteCount(data);

            if (fill.HasValue && data.Length < length)
                data = padLeft ? data.PadLeft(length, fill.Value) : data.PadRight(length, fill.Value);

            var result = Encoding.UTF8.GetBytes(data);
            var full = new byte[length];

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
            var result = GetBytes(data, length, out byteLength, '0', padLeft);

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
            return GetHexBytes(data.Select(x => x.ToString("X2")).Aggregate((a, b) => a + b), length, out byteLength,
                padLeft);
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
        public delegate Error MposInitializedCallbackDelegate(IntPtr mpos, int error);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Error MposPaymentCallbackDelegateInterop(IntPtr mpos, int error, IntPtr info);
        public delegate Error MposPaymentCallbackDelegate(IntPtr mpos, int error, PaymentInfo info);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Error MposTablesLoadedCallbackDelegate(IntPtr mpos, int error, bool loaded);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Error MposFinishTransactionCallbackDelegate(IntPtr mpos, int error);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Error MposExtractKeysCallbackDelegateInterop(IntPtr mpos, int error, IntPtr keys, int keysLength);
        public delegate Error MposExtractKeysCallbackDelegate(IntPtr mpos, int error, int[] keyList);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Error MposGetTableVersionCallbackDelegateInterop(IntPtr mpos, int error, IntPtr version);
        public delegate Error MposGetTableVersionCallbackDelegate(IntPtr mpos, int error, String version);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Error MposClosedCallbackDelegate(IntPtr mpos, int error);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Error TmsStoreCallbackDelegateInterop(
            string version, IntPtr tables, int tableLen, IntPtr applications, int appLen, IntPtr riskManagement,
            int riskmanLen, IntPtr acquirers, int acqLen, IntPtr userData);
        public delegate Error TmsStoreCallbackDelegate(
            string version, IList<Capk> capkList, IList<Aid> aidList, IList<Application> appList,
            IList<RiskManagement> riskProfileList, IList<Acquirer> acquirerList, IntPtr userData);


        private readonly static INativeImport Dll = Environment.Is64BitProcess ? NativeBit64.Dll : NativeBit32.Dll;

        public static IntPtr Create(AbecsStream stream, MposNotificationCallbackDelegate notificationCallback, MposOperationCompletedCallbackDelegate operationCompletedCallback)
        {
            return Dll.Create(stream, notificationCallback, operationCompletedCallback);
        }

        public static Error Initialize(IntPtr mpos, MposInitializedCallbackDelegate initializedCallback)
        {
            return Dll.Initialize(mpos, initializedCallback);
        }




        public static Error ProcessPayment(IntPtr mpos, int amount, Application[] applicationList, int applicationListLength, Acquirer[] acquirers, int acquirerListLength, RiskManagement[] riskManagementList, int riskManagementListLength, int magstripePaymentMethod, MposPaymentCallbackDelegate paymentCallback, bool contactlessDisabled)
        {
            return Dll.ProcessPayment(mpos, amount, applicationList, applicationListLength, acquirers, acquirerListLength, riskManagementList, riskManagementListLength, magstripePaymentMethod, paymentCallback, contactlessDisabled);
        }



        public static Error UpdateTables(Mpos mpos, MposTablesLoadedCallbackDelegate tableCallback, Aid[] aidList, Capk[] capkList)
        {
            return Dll.UpdateTables(
                mpos.nativeMpos, mpos.TMSStorage.GetGlobalVersion(), true,
                tableCallback, aidList, capkList
            );
        }

        public static Error FinishTransaction(IntPtr mpos, TransactionStatus status, int arc, string emv, int emvLen, MposFinishTransactionCallbackDelegate callback)
        {
            return Dll.FinishTransaction(mpos, status, arc, emv, emvLen, callback);
        }

        public static Error ExtractKeys(IntPtr mpos, MposExtractKeysCallbackDelegate callback)
        {
            return Dll.ExtractKeys(mpos, callback);
        }

        public static Error GetTableVersion(IntPtr mpos, MposGetTableVersionCallbackDelegate callback)
        {
            return Dll.GetTableVersion(mpos, callback);
        }

        public static Error Display(IntPtr mpos, string text)
        {
            return Dll.Display(mpos, text);
        }

        public static Error Close(IntPtr mpos, string text, MposClosedCallbackDelegate callback)
        {
            return Dll.Close(mpos, text, callback);
        }

        public static Error Free(IntPtr mpos)
        {
            return Dll.Free(mpos);
        }

        public static Error Cancel(IntPtr mpos)
        {
            return Dll.Cancel(mpos);
        }

        public static Error TmsGetTables(string payload, int length, TmsStoreCallbackDelegate callback, IntPtr userData)
        {
            return Dll.TmsGetTables(payload, length, callback, userData);
        }
    }
}
