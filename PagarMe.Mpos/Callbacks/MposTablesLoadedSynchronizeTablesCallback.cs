using PagarMe.Mpos.Helpers;
using System;
using System.Threading.Tasks;
using static PagarMe.Mpos.Mpos;

namespace PagarMe.Mpos.Callbacks
{
    class MposTablesLoadedSynchronizeTablesCallback
    {
        public static Native.MposTablesLoadedCallbackDelegate Callback(Mpos mpos, TaskCompletionSource<Boolean> source)
        {
            return GCHelper.ManualFree<Native.MposTablesLoadedCallbackDelegate>(releaseGC =>
            {
                return (mposPtr, tableError, loaded) =>
                {
                    releaseGC();
                    return callback(mpos, source, tableError, loaded);
                };
            });
        }

        private static Native.Error callback(Mpos mpos, TaskCompletionSource<bool> source, int tableError, bool loaded)
        {
            mpos.OnTableUpdated(loaded, tableError);
            source.SetResult(true);

            return Native.Error.Ok;
        }
    }

}