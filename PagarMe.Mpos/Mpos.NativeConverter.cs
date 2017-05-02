using PagarMe.Mpos.Abecs;
using PagarMe.Mpos.Callbacks;
using PagarMe.Mpos.Helpers;
using System;
using System.Collections;
using System.Linq;
using static PagarMe.Mpos.Mpos.Native;

namespace PagarMe.Mpos
{
    public partial class Mpos
    {
        internal class NativeConverter
        {
            protected TmsStoreCallbackDelegateInterop Convert(TmsStoreCallbackDelegate callback)
            {
                return new TmsStoreToInterop(callback).Convert();
            }

            protected MposPaymentCallbackDelegateInterop Convert(MposPaymentCallbackDelegate callback)
            {
                return new MposPaymentToInterop(callback).Convert();
            }

            protected MposExtractKeysCallbackDelegateInterop Convert(MposExtractKeysCallbackDelegate callback)
            {
                return new MposExtractKeysToInterop(callback).Convert();
            }

            protected MposGetTableVersionCallbackDelegateInterop Convert(MposGetTableVersionCallbackDelegate callback)
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
}