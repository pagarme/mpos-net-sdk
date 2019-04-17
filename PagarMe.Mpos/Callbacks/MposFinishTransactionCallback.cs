using System.Threading.Tasks;
using PagarMe.Mpos.Entities;
using PagarMe.Mpos.Natives;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos.Callbacks
{
    class MposFinishTransactionCallback
    {
        public static MposFinishTransactionCallbackDelegate Callback(Mpos mpos, TaskCompletionSource<bool> source)
        {
            return GCHelper.ManualFree<MposFinishTransactionCallbackDelegate>(releaseGC =>
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
            mpos.OnFinishedTransaction(err);
            source.SetResult(true);

            return Error.Ok;
        }
    }
}
