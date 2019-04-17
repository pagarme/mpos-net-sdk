using System;
using System.Threading.Tasks;
using PagarMe.Mpos.Api;
using PagarMe.Mpos.Entities;
using PagarMe.Mpos.Natives;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos.Callbacks
{
    class MposExtractKeysCallback
    {
        private int[] keyList;

        public static MposExtractKeysCallbackDelegate Callback(Mpos mpos, bool forceUpdate, TaskCompletionSource<bool> source)
        {
            return GCHelper.ManualFree<MposExtractKeysCallbackDelegate>(releaseGC =>
            {
                return (mposPtr, err, keyList) =>
                {
                    releaseGC();

                    var instance = new MposExtractKeysCallback();

                    instance.keyList = keyList;

                    return instance.processTables(mpos, forceUpdate, source);
                };
            });
        }

        private Error processTables(Mpos mpos, bool forceUpdate, TaskCompletionSource<bool> source)
        {
            var tmsCallback = TmsStoreCallback.Callback(mpos, forceUpdate, source);

            ApiHelper.GetTerminalTables(mpos.EncryptionKey, !forceUpdate ? mpos.TMSStorage.GetGlobalVersion() : "", keyList)
                .ContinueWith(t =>
                {
                    if (t.Status == TaskStatus.Faulted)
                    {
                        source.SetException(t.Exception);
                        return;
                    }

                    if (t.Result.Length > 0)
                    {
                        try
                        {
                            var error = TmsGetTables(t.Result, t.Result.Length, tmsCallback, IntPtr.Zero);
                            if (error != Error.Ok) throw new MposException(error);
                        }
                        catch(Exception e)
                        {
                            source.SetException(e);
                        }
                    }

                    else
                    {
                        // We don't need to do anything; complete operation.	
                        mpos.OnTableUpdated(false, 0);
                        source.SetResult(true);
                    }
                });

            return Error.Ok;
        }
    }
}
