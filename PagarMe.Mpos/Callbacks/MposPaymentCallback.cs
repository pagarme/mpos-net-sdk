using PagarMe.Mpos.Helpers;
using System.Threading.Tasks;
using static PagarMe.Mpos.Mpos;

namespace PagarMe.Mpos.Callbacks
{
    class MposPaymentCallback
    {
        private Native.PaymentInfo info;

        public static Native.MposPaymentCallbackDelegate Callback(Mpos mpos, TaskCompletionSource<PaymentResult> source)
        {
            return GCHelper.ManualFree<Native.MposPaymentCallbackDelegate>(releaseGC =>
            {
                return (mposPtr, err, info) =>
                {
                    releaseGC();

                    var instance = new MposPaymentCallback();
                    instance.info = info;

                    return instance.handleResult(mpos, source, err);
                };
            });
        }

        private Native.Error handleResult(Mpos mpos, TaskCompletionSource<PaymentResult> source, int err)
        {
            if (err != 0)
            {
                var result = new PaymentResult();
                result.BuildErrored(err);
                source.SetResult(result);

                mpos.OnPaymentProcessed(null, err);
                return Native.Error.Ok;
            }

            mpos.HandlePaymentCallback(err, info).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.Faulted) source.SetException(t.Exception);
                else source.SetResult(t.Result);

                mpos.OnPaymentProcessed(t.Result, err);
            });

            return Native.Error.Ok;
        }
    }
}