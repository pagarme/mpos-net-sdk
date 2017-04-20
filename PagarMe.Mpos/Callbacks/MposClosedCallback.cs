using PagarMe.Mpos.Helpers;
using System.Threading.Tasks;
using static PagarMe.Mpos.Mpos;

namespace PagarMe.Mpos.Callbacks
{
    class MposClosedCallback
    {
        public static Native.MposClosedCallbackDelegate Callback(Mpos mpos, TaskCompletionSource<bool> source)
        {
            return GCHelper.ManualFree<Native.MposClosedCallbackDelegate>(releaseGC =>
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
            mpos.OnClosed(err);
            source.SetResult(true);

            return Native.Error.Ok;
        }
    }
}
