using PagarMe.Mpos.Abecs;
using PagarMe.Mpos.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static PagarMe.Mpos.Mpos.Native;

namespace PagarMe.Mpos
{
    public partial class Mpos
    {
        internal class NativeBit
        {
            protected TmsStoreCallbackDelegateInterop Convert(TmsStoreCallbackDelegate callback)
            {
                return GCHelper.ManualFree<TmsStoreCallbackDelegateInterop>(releaseGC =>
                {
                    return (version, tables, tableLen, applications, appLen, riskManagement, riskmanLen, acquirers, acqLen, userData) =>
                    {
                        releaseGC();

                        var capkList = new List<Capk>();
                        var aidList = new List<Aid>();

                        for (var i = 0; i < tableLen; i++)
                        {
                            // We assume everything is the smaller member
                            var capk = PtrHelper.DoubleDeref<Capk>(tables, i);
                            var isAid = capk.IsAid;

                            if (isAid)
                            {
                                var aid = PtrHelper.DoubleDeref<Aid>(tables, i);
                                aidList.Add(aid);
                            }
                            else
                            {
                                capkList.Add(capk);
                            }
                        }

                        var appList = PtrHelper.DoubleDerefList<Application>(applications, appLen);
                        var riskProfileList = PtrHelper.DoubleDerefList<RiskManagement>(riskManagement, riskmanLen);
                        var acquirerList = PtrHelper.DoubleDerefList<Acquirer>(acquirers, acqLen);

                        return callback(version, capkList, aidList, appList, riskProfileList, acquirerList, userData);
                    };
                });
            }


            protected MposPaymentCallbackDelegateInterop Convert(MposPaymentCallbackDelegate callback)
            {
                return GCHelper.ManualFree<MposPaymentCallbackDelegateInterop>(releaseGC =>
                {
                    return (mpos, error, infoPointer) =>
                    {
                        releaseGC();

                        var info = PtrHelper.Deref<PaymentInfo>(infoPointer);
                        return callback(mpos, error, info);
                    };
                });
            }


            protected MposExtractKeysCallbackDelegateInterop Convert(MposExtractKeysCallbackDelegate callback)
            {
                return GCHelper.ManualFree<MposExtractKeysCallbackDelegateInterop>(releaseGC =>
                {
                    return (mpos, error, keys, keysLength) =>
                    {
                        releaseGC();

                        var keyList = PtrHelper.DerefList<int>(keys, keysLength).ToArray();
                        return callback(mpos, error, keyList);
                    };
                });
            }


            protected MposGetTableVersionCallbackDelegateInterop Convert(MposGetTableVersionCallbackDelegate callback)
            {
                return GCHelper.ManualFree<MposGetTableVersionCallbackDelegateInterop>(releaseGC =>
                {
                    return (mpos, error, version) =>
                    {
                        releaseGC();

                        var cleanVersionBytes = PtrHelper.DerefList<byte>(version, 10).ToArray();
                        var cleanVersion = GetString(cleanVersionBytes);
                        return callback(mpos, error, cleanVersion);
                    };
                });
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