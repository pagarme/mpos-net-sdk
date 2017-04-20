using PagarMe.Mpos.Helpers;
using System;
using System.Threading.Tasks;
using static PagarMe.Mpos.Mpos;

namespace PagarMe.Mpos.Callbacks
{
    class MposInitializedCallback
    {
        public static Native.MposInitializedCallbackDelegate Callback(Mpos mpos, TaskCompletionSource<bool> source)
        {
            return GCHelper.ManualFree<Native.MposInitializedCallbackDelegate>(releaseGC =>
            {
                return (mposPtr, err) =>
                {
                    releaseGC();
                    return callback(mpos, source, err);
                };
            });

        }

        private static Native.Error callback(Mpos mpos, TaskCompletionSource<bool> source, int err)
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

            return Native.Error.Ok;
        }
    }
}
