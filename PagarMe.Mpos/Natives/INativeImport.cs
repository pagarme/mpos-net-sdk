using System;
using System.Collections;
using PagarMe.Mpos.Abecs;
using PagarMe.Mpos.Entities;

namespace PagarMe.Mpos.Natives
{
    internal interface INativeImport
    {
        IntPtr Create(AbecsStream stream, Native.MposNotificationCallbackDelegate notificationCallback,
                Native.MposOperationCompletedCallbackDelegate operationCompletedCallback);

        Error Initialize(IntPtr mpos, Native.MposInitializedCallbackDelegate initializedCallback);

	    Error ProcessPayment(IntPtr mpos, int amount, Application[] applicationList,
            int applicationListLength, Acquirer[] acquirers, int acquirerListLength,
            RiskManagement[] riskManagementList, int riskManagementListLength, int magstripePaymentMethod,
            Native.MposPaymentCallbackDelegate paymentCallback);

	    Error UpdateTables(IntPtr mpos, string version, bool forceUpdate, 
            Native.MposTablesLoadedCallbackDelegate callback, params IList[] data);

	    Error FinishTransaction(IntPtr mpos, TransactionStatus status, int arc, string emv,
            int emvLen, Native.MposFinishTransactionCallbackDelegate callback);

	    Error ExtractKeys(IntPtr mpos, Native.MposExtractKeysCallbackDelegate callback);

	    Error GetTableVersion(IntPtr mpos, Native.MposGetTableVersionCallbackDelegate callback);

	    Error Display(IntPtr mpos, string text);

	    Error Close(IntPtr mpos, string text, Native.MposClosedCallbackDelegate callback);

	    Error Free(IntPtr mpos);

	    Error Cancel(IntPtr mpos);

	    Error TmsGetTables(string payload, int length, Native.TmsStoreCallbackDelegate callback, IntPtr mpos);
    }
}