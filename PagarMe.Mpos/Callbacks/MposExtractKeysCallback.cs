using PagarMe.Mpos.Helpers;
using System;
using System.Threading.Tasks;
using static PagarMe.Mpos.Mpos;

namespace PagarMe.Mpos.Callbacks
{
    class MposExtractKeysCallback
    {
        private int[] keyList;

        public static Native.MposExtractKeysCallbackDelegate Callback(Mpos mpos, bool forceUpdate, TaskCompletionSource<bool> source)
        {
            return GCHelper.ManualFree<Native.MposExtractKeysCallbackDelegate>(releaseGC =>
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

        private Native.Error processTables(Mpos mpos, bool forceUpdate, TaskCompletionSource<bool> source)
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
                        var error = Native.TmsGetTables(t.Result, t.Result.Length, tmsCallback, IntPtr.Zero);
                        if (error != Native.Error.Ok) throw new MposException(error);
                    }

                    else
                    {
                        // We don't need to do anything; complete operation.	
                        mpos.OnTableUpdated(false, 0);
                        source.SetResult(true);
                    }
                });

            return Native.Error.Ok;
        }
    }
}
