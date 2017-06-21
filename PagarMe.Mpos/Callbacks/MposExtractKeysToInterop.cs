using PagarMe.Mpos.Helpers;
using static PagarMe.Mpos.Mpos.Native;

namespace PagarMe.Mpos.Callbacks
{
    internal class MposExtractKeysToInterop : GCEnvelop<MposExtractKeysCallbackDelegate, MposExtractKeysCallbackDelegateInterop>
    {
        public MposExtractKeysToInterop(MposExtractKeysCallbackDelegate callback) : base(callback) { }

        protected override MposExtractKeysCallbackDelegateInterop convert => (mpos, error, keys, keysLength) =>
        {
            releaseGC();

            var keyList = PtrHelper.DerefList<int>(keys, keysLength).ToArray();
            return callback(mpos, error, keyList);
        };
    }
}
