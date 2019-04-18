using System;
using System.Collections;
using System.Linq;
using PagarMe.Mpos.Abecs;
using PagarMe.Mpos.Callbacks;
using PagarMe.Mpos.Entities;

namespace PagarMe.Mpos.Natives
{
    internal class NativeConverter
    {
        protected Native.TmsStoreCallbackDelegateInterop Convert(Native.TmsStoreCallbackDelegate callback)
        {
            return new TmsStoreToInterop(callback).Convert();
        }

        protected Native.MposPaymentCallbackDelegateInterop Convert(Native.MposPaymentCallbackDelegate callback)
        {
            return new MposPaymentToInterop(callback).Convert();
        }

        protected Native.MposExtractKeysCallbackDelegateInterop Convert(Native.MposExtractKeysCallbackDelegate callback)
        {
            return new MposExtractKeysToInterop(callback).Convert();
        }

        protected Native.MposGetTableVersionCallbackDelegateInterop Convert(Native.MposGetTableVersionCallbackDelegate callback)
        {
            return new MposGetTableVersionToInterop(callback).Convert();
        }

        protected Error Convert(IList[] dataList, Func<IntPtr, Int32, Error> externCall)
        {
            var data = PtrHelper.RefLists(dataList);
            var count = dataList.Sum(e => e.Count);

            var result = externCall(data, count);

            PtrHelper.FreeLists(data, dataList);

            return result;
        }

        protected unsafe IntPtr Convert(AbecsStream stream)
        {
            return (IntPtr)stream.NativeStream;
        }

    }
}
