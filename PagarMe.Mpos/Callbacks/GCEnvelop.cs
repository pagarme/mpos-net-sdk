using PagarMe.Mpos.Helpers;
using System;

namespace PagarMe.Mpos.Callbacks
{
    internal abstract class GCEnvelop<TDelegate, TDelegateInterop>
    {
        protected Action releaseGC { get; private set; }
        protected TDelegate callback { get; private set; }
        protected abstract TDelegateInterop convert { get; }

        protected GCEnvelop(TDelegate callback)
        {
            this.callback = callback;
        }

        public TDelegateInterop Convert()
        {
            return GCHelper.ManualFree(releaseGC =>
            {
                this.releaseGC = releaseGC;
                return convert;
            });
        }
    }

}
