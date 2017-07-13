using PagarMe.Mpos.Helpers;
using static PagarMe.Mpos.Mpos.Native;

namespace PagarMe.Mpos.Callbacks
{
    internal class MposPaymentToInterop : GCEnvelop<MposPaymentCallbackDelegate, MposPaymentCallbackDelegateInterop>
    {
        public MposPaymentToInterop(MposPaymentCallbackDelegate callback) : base(callback) { }

        protected override MposPaymentCallbackDelegateInterop convert => (mpos, error, infoPointer) =>
        {
            releaseGC();

            var info = PtrHelper.DerefOrDefault<PaymentInfo>(infoPointer);
            return callback(mpos, error, info);
        };
    }
}
