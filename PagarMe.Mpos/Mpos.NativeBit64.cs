using PagarMe.Mpos.Abecs;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using static PagarMe.Mpos.Mpos.Native;

namespace PagarMe.Mpos
{
    public partial class Mpos
    {
        class NativeBit64 : NativeConverter, INativeImport
        {
            const String mpos = "mpos64";
            const String tms = "tms64";

            public static INativeImport Dll = new NativeBit64();
            private NativeBit64() { }

            [DllImport(mpos, EntryPoint = "mpos_new", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr CreateExtern(IntPtr stream, MposNotificationCallbackDelegate notificationCallback, MposOperationCompletedCallbackDelegate operationCompletedCallback);
            public IntPtr Create(AbecsStream stream, MposNotificationCallbackDelegate notificationCallback, MposOperationCompletedCallbackDelegate operationCompletedCallback)
            {
                return CreateExtern(Convert(stream), notificationCallback, operationCompletedCallback);
            }

            [DllImport(mpos, EntryPoint = "mpos_initialize", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern Error InitializeExtern(IntPtr mpos, IntPtr streamData, MposInitializedCallbackDelegate initializedCallback);
            public Error Initialize(IntPtr mpos, MposInitializedCallbackDelegate initializedCallback)
            {
                return InitializeExtern(mpos, IntPtr.Zero, initializedCallback);
            }

            [DllImport(mpos, EntryPoint = "mpos_process_payment", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern Error ProcessPaymentExtern(IntPtr mpos, int amount, Application[] applicationList, int applicationListLength, Acquirer[] acquirers, int acquirerListLength, RiskManagement[] riskManagementList, int riskManagementListLength, int magstripePaymentMethod, MposPaymentCallbackDelegateInterop paymentCallback);
            public Error ProcessPayment(IntPtr mpos, int amount, Application[] applicationList, int applicationListLength, Acquirer[] acquirers, int acquirerListLength, RiskManagement[] riskManagementList, int riskManagementListLength, int magstripePaymentMethod, MposPaymentCallbackDelegate paymentCallback)
            {
                return ProcessPaymentExtern(mpos, amount, applicationList, applicationListLength, acquirers, acquirerListLength, riskManagementList, riskManagementListLength, magstripePaymentMethod, Convert(paymentCallback));
            }

            [DllImport(mpos, EntryPoint = "mpos_update_tables", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern Error UpdateTablesExtern(IntPtr mpos, IntPtr data, int count, string version, bool forceUpdate, MposTablesLoadedCallbackDelegate callback);
            public Error UpdateTables(IntPtr mpos, string version, bool forceUpdate, MposTablesLoadedCallbackDelegate callback, IList[] dataList)
            {
                return Convert(dataList, (data, count) =>
                {
                    return UpdateTablesExtern(mpos, data, count, version, forceUpdate, callback);
                });
            }

            [DllImport(mpos, EntryPoint = "mpos_finish_transaction", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern Error FinishTransactionExtern(IntPtr mpos, TransactionStatus status, int arc, string emv, int emvLen, MposFinishTransactionCallbackDelegate callback);
            public Error FinishTransaction(IntPtr mpos, TransactionStatus status, int arc, string emv, int emvLen, MposFinishTransactionCallbackDelegate callback)
            {
                return FinishTransactionExtern(mpos, status, arc, emv, emvLen, callback);
            }

            [DllImport(mpos, EntryPoint = "mpos_extract_keys", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern Error ExtractKeysExtern(IntPtr mpos, MposExtractKeysCallbackDelegateInterop callback);
            public Error ExtractKeys(IntPtr mpos, MposExtractKeysCallbackDelegate callback)
            {
                return ExtractKeysExtern(mpos, Convert(callback));
            }

            [DllImport(mpos, EntryPoint = "mpos_get_table_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
            public static extern Error GetTableVersionExtern(IntPtr mpos, MposGetTableVersionCallbackDelegateInterop callback);
            public Error GetTableVersion(IntPtr mpos, MposGetTableVersionCallbackDelegate callback)
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
            public static extern Error CloseExtern(IntPtr mpos, string text, MposClosedCallbackDelegate callback);
            public Error Close(IntPtr mpos, string text, MposClosedCallbackDelegate callback)
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
            public static extern Error TmsGetTablesExtern(string payload, int length, TmsStoreCallbackDelegateInterop callback, IntPtr userData);
            public Error TmsGetTables(string payload, int length, TmsStoreCallbackDelegate callback, IntPtr mpos)
            {
                return TmsGetTablesExtern(payload, length, Convert(callback), mpos);
            }
        }
    }
}