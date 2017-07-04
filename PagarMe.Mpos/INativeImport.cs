using PagarMe.Mpos.Abecs;
using System;
using System.Collections;
using static PagarMe.Mpos.Mpos.Native;

namespace PagarMe.Mpos
{
    internal interface INativeImport
    {
        IntPtr Create(AbecsStream stream, MposNotificationCallbackDelegate notificationCallback,
                MposOperationCompletedCallbackDelegate operationCompletedCallback);

        Error Initialize(IntPtr mpos, MposInitializedCallbackDelegate initializedCallback);

        Error ProcessPayment(IntPtr mpos, int amount, Application[] applicationList,
            int applicationListLength, Acquirer[] acquirers, int acquirerListLength,
            RiskManagement[] riskManagementList, int riskManagementListLength, int magstripePaymentMethod,
            MposPaymentCallbackDelegate paymentCallback);

        Error UpdateTables(IntPtr mpos, string version, bool forceUpdate, 
            MposTablesLoadedCallbackDelegate callback, params IList[] data);

        Error FinishTransaction(IntPtr mpos, TransactionStatus status, int arc, string emv,
            int emvLen, MposFinishTransactionCallbackDelegate callback);

        Error ExtractKeys(IntPtr mpos, MposExtractKeysCallbackDelegate callback);

        Error GetTableVersion(IntPtr mpos, MposGetTableVersionCallbackDelegate callback);

        Error Display(IntPtr mpos, string text);

        Error Close(IntPtr mpos, string text, MposClosedCallbackDelegate callback);

        Error Free(IntPtr mpos);

        Error Cancel(IntPtr mpos);

        Error TmsGetTables(string payload, int length, TmsStoreCallbackDelegate callback, IntPtr mpos);
    }
}