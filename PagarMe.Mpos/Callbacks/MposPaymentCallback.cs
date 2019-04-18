using System.Threading.Tasks;
using PagarMe.Mpos.Entities;
using PagarMe.Mpos.Natives;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos.Callbacks
{
    class MposPaymentCallback
    {
        private PaymentInfo info;

        public static MposPaymentCallbackDelegate Callback(Mpos mpos, TaskCompletionSource<PaymentResult> source)
        {
            return GCHelper.ManualFree<MposPaymentCallbackDelegate>(releaseGC =>
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

        private Error handleResult(Mpos mpos, TaskCompletionSource<PaymentResult> source, int err)
        {
            if (err != 0)
            {
                var result = new PaymentResult();
                result.BuildErrored(err);
                source.SetResult(result);

                mpos.OnPaymentProcessed(null, err);
                return Error.Ok;
            }

            mpos.HandlePaymentCallback(err, info).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.Faulted) source.SetException(t.Exception);
                else source.SetResult(t.Result);

                mpos.OnPaymentProcessed(t.Result, err);
            });

            return Error.Ok;
        }
    }
}
