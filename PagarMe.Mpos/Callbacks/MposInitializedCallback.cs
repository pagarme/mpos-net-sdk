using System;
using System.Threading.Tasks;
using PagarMe.Mpos.Entities;
using PagarMe.Mpos.Natives;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos.Callbacks
{
    class MposInitializedCallback
    {
        public static MposInitializedCallbackDelegate Callback(Mpos mpos, TaskCompletionSource<bool> source)
        {
            return GCHelper.ManualFree<MposInitializedCallbackDelegate>(releaseGC =>
            {
                return (mposPtr, err) =>
                {
                    releaseGC();
                    return callback(mpos, source, err);
                };
            });

        }

        private static Error callback(Mpos mpos, TaskCompletionSource<bool> source, int err)
        {
            try
            {
                mpos.OnInitialized(err);
                source.TrySetResult(true);
            }
            catch (Exception ex)
            {
                source.TrySetException(ex);
            }

            return Error.Ok;
        }
    }
}
