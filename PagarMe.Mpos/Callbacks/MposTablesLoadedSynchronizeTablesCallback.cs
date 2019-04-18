using System;
using System.Threading.Tasks;
using PagarMe.Mpos.Entities;
using PagarMe.Mpos.Natives;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos.Callbacks
{
    class MposTablesLoadedSynchronizeTablesCallback
    {
        public static MposTablesLoadedCallbackDelegate Callback(Mpos mpos, TaskCompletionSource<Boolean> source)
        {
            return GCHelper.ManualFree<MposTablesLoadedCallbackDelegate>(releaseGC =>
            {
                return (mposPtr, tableError, loaded) =>
                {
                    releaseGC();
                    return callback(mpos, source, tableError, loaded);
                };
            });
        }

        private static Error callback(Mpos mpos, TaskCompletionSource<bool> source, int tableError, bool loaded)
        {
            mpos.OnTableUpdated(loaded, tableError);
            source.SetResult(true);

            return Error.Ok;
        }
    }

}
