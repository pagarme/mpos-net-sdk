using PagarMe.Mpos;
using System;
using static PagarMe.Mpos.Mpos.Native;
using transactionStatus = PagarMe.Mpos.Mpos.Native.TransactionStatus;
using System.Collections;
using PagarMe.Mpos.Abecs;
using PagarMe.Mpos.Helpers;

namespace PagarMe.Mock.LibC
{
    partial class DllMock : INativeImport
    {
        AbecsStream stream;

        public IntPtr Create(AbecsStream stream, MposNotificationCallbackDelegate notificationCallback, MposOperationCompletedCallbackDelegate operationCompletedCallback)
        {
            this.stream = stream;

            mpos = PtrHelper.Ref(pointer);

            notificationCallback(mpos, notification);
            operationCompletedCallback(mpos);

            return mpos;
        }

        public Error Cancel(IntPtr mpos)
        {
            ReceivedMpos = mpos;

            return result;
        }

        public Error Close(IntPtr mpos, string text, MposClosedCallbackDelegate callback)
        {
            ReceivedMpos = mpos;

            callback(mpos, error);

            return result;
        }

        public Error Display(IntPtr mpos, string text)
        {
            ReceivedMpos = mpos;

            return result;
        }

        public Error ExtractKeys(IntPtr mpos, MposExtractKeysCallbackDelegate callback)
        {
            write("ExtractKeys");

            ReceivedMpos = mpos;

            callback(mpos, error, keyList);

            return result;
        }

        public Error FinishTransaction(IntPtr mpos, transactionStatus status, int arc, string emv, int emvLen, MposFinishTransactionCallbackDelegate callback)
        {
            ReceivedMpos = mpos;

            callback(mpos, error);

            return result;
        }

        public Error Free(IntPtr mpos)
        {
            ReceivedMpos = mpos;

            return result;
        }

        public Error GetTableVersion(IntPtr mpos, MposGetTableVersionCallbackDelegate callback)
        {
            write("GetTableVersion");

            ReceivedMpos = mpos;

            callback(mpos, error, version);

            return result;
        }

        public Error Initialize(IntPtr mpos, MposInitializedCallbackDelegate initializedCallback)
        {
            if (resultInit != Error.Ok)
                return resultInit;

            unsafe { stream.Open(stream.NativeStream); }
            write("Initialize");

            ReceivedMpos = mpos;
            initializedCallback(mpos, error);

            return resultInit;
        }

        private void write(string text)
        {
            unsafe
            {
                var ptr = PtrHelper.Ref(text);
                stream.Write(stream.NativeStream, ptr, text.Length);
            }
        }

        public Error ProcessPayment(IntPtr mpos, int amount, Application[] applicationList, int applicationListLength, Acquirer[] acquirers, int acquirerListLength, RiskManagement[] riskManagementList, int riskManagementListLength, int magstripePaymentMethod, MposPaymentCallbackDelegate paymentCallback)
        {
            write("ProcessPayment");

            ReceivedMpos = mpos;

            paymentCallback(mpos, error, info);

            return result;
        }

        public Error TmsGetTables(string payload, int length, TmsStoreCallbackDelegate callback, IntPtr userData)
        {
            write("TmsGetTables");

            ReceivedMpos = mpos;

            callback(version, capkList, aidList, appList, riskProfileList, acquirerList, userData);

            return result;
        }

        public Error UpdateTables(IntPtr mpos, string version, bool force_update, MposTablesLoadedCallbackDelegate callback, IList[] dataList)
        {
            if (resultTableUpdate != Error.Ok)
                return resultTableUpdate;

            write("UpdateTables");

            ReceivedMpos = mpos;

            callback(mpos, error, loaded);

            return resultTableUpdate;
        }

        

        ~DllMock()
        {
            PtrHelper.Free(mpos);
        }

    }
}
