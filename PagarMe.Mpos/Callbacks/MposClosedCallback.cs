using System.Threading.Tasks;
using PagarMe.Mpos.Entities;
using PagarMe.Mpos.Natives;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos.Callbacks
{
    class MposClosedCallback
    {
        public static MposClosedCallbackDelegate Callback(Mpos mpos, TaskCompletionSource<bool> source)
        {
            return GCHelper.ManualFree<MposClosedCallbackDelegate>(releaseGC =>
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
            mpos.OnClosed(err);
            source.SetResult(true);

            return Error.Ok;
        }
    }
}
