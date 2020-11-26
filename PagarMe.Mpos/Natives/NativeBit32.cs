using System;
using System.Collections;
using System.Runtime.InteropServices;
using PagarMe.Mpos.Abecs;
using PagarMe.Mpos.Entities;

namespace PagarMe.Mpos.Natives
{
    class NativeBit32 : NativeConverter, INativeImport
    {
        const String mpos = "mpos32";
        const String tms = "tms32";

        public static INativeImport Dll = new NativeBit32();
        private NativeBit32() { }

        [DllImport(mpos, EntryPoint = "mpos_new", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateExtern(IntPtr stream, Native.MposNotificationCallbackDelegate notificationCallback, Native.MposOperationCompletedCallbackDelegate operationCompletedCallback);
        public IntPtr Create(AbecsStream stream, Native.MposNotificationCallbackDelegate notificationCallback, Native.MposOperationCompletedCallbackDelegate operationCompletedCallback)
        {
            return CreateExtern(Convert(stream), notificationCallback, operationCompletedCallback);
        }

        [DllImport(mpos, EntryPoint = "mpos_initialize", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Error InitializeExtern(IntPtr mpos, IntPtr streamData, Native.MposInitializedCallbackDelegate initializedCallback);
        public Error Initialize(IntPtr mpos, Native.MposInitializedCallbackDelegate initializedCallback)
        {
            return InitializeExtern(mpos, IntPtr.Zero, initializedCallback);
        }

        [DllImport(mpos, EntryPoint = "mpos_process_payment", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Error ProcessPaymentExtern(IntPtr mpos, int amount, Application[] applicationList, int applicationListLength, Acquirer[] acquirers, int acquirerListLength, RiskManagement[] riskManagementList, int riskManagementListLength, int magstripePaymentMethod, Native.MposPaymentCallbackDelegateInterop paymentCallback, bool contactlessDisabled);
        public Error ProcessPayment(IntPtr mpos, int amount, Application[] applicationList, int applicationListLength, Acquirer[] acquirers, int acquirerListLength, RiskManagement[] riskManagementList, int riskManagementListLength, int magstripePaymentMethod, Native.MposPaymentCallbackDelegate paymentCallback, bool  contactlessDisabled)
        {
            return ProcessPaymentExtern(mpos, amount, applicationList, applicationListLength, acquirers, acquirerListLength, riskManagementList, riskManagementListLength, magstripePaymentMethod, Convert(paymentCallback), contactlessDisabled);
        }

        [DllImport(mpos, EntryPoint = "mpos_update_tables", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Error UpdateTablesExtern(IntPtr mpos, IntPtr data, int count, string version, bool forceUpdate, Native.MposTablesLoadedCallbackDelegate callback);
        public Error UpdateTables(IntPtr mpos, string version, bool forceUpdate, Native.MposTablesLoadedCallbackDelegate callback, params IList[] dataList)
        {
            return Convert(dataList, (data, count) => {
                return UpdateTablesExtern(mpos, data, count, version, forceUpdate, callback);
            });
        }

        [DllImport(mpos, EntryPoint = "mpos_finish_transaction", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Error FinishTransactionExtern(IntPtr mpos, TransactionStatus status, int arc, string emv, int emvLen, Native.MposFinishTransactionCallbackDelegate callback);
        public Error FinishTransaction(IntPtr mpos, TransactionStatus status, int arc, string emv, int emvLen, Native.MposFinishTransactionCallbackDelegate callback)
        {
            return FinishTransactionExtern(mpos, status, arc, emv, emvLen, callback);
        }

        [DllImport(mpos, EntryPoint = "mpos_extract_keys", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Error ExtractKeysExtern(IntPtr mpos, Native.MposExtractKeysCallbackDelegateInterop callback);
        public Error ExtractKeys(IntPtr mpos, Native.MposExtractKeysCallbackDelegate callback)
        {
            return ExtractKeysExtern(mpos, Convert(callback));
        }

        [DllImport(mpos, EntryPoint = "mpos_get_table_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Error GetTableVersionExtern(IntPtr mpos, Native.MposGetTableVersionCallbackDelegateInterop callback);
        public Error GetTableVersion(IntPtr mpos, Native.MposGetTableVersionCallbackDelegate callback)
        {
            return GetTableVersionExtern(mpos, Convert(callback));
        }

        [DllImport(mpos, EntryPoint = "mpos_display", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Error DisplayExtern(IntPtr mpos, string text);
        public Error Display(IntPtr mpos, string text)
        {
            return DisplayExtern(mpos, text);
        }

        [DllImport(mpos, EntryPoint = "mpos_close", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Error CloseExtern(IntPtr mpos, string text, Native.MposClosedCallbackDelegate callback);
        public Error Close(IntPtr mpos, string text, Native.MposClosedCallbackDelegate callback)
        {
            return CloseExtern(mpos, text, callback);
        }

        [DllImport(mpos, EntryPoint = "mpos_free", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Error FreeExtern(IntPtr mpos);
        public Error Free(IntPtr mpos)
        {
            return FreeExtern(mpos);
        }

        [DllImport(mpos, EntryPoint = "mpos_cancel", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Error CancelExtern(IntPtr mpos);
        public Error Cancel(IntPtr mpos)
        {
            return CancelExtern(mpos);
        }

        [DllImport(tms, EntryPoint = "tms_get_tables", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern Error TmsGetTablesExtern(string payload, int length, Native.TmsStoreCallbackDelegateInterop callback, IntPtr userData);
        public Error TmsGetTables(string payload, int length, Native.TmsStoreCallbackDelegate callback, IntPtr mpos)
        {
            return TmsGetTablesExtern(payload, length, Convert(callback), mpos);
        }

    }
}
