using PagarMe.Mpos.Natives;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos.Callbacks
{
    internal class MposGetTableVersionToInterop : GCEnvelop<MposGetTableVersionCallbackDelegate, MposGetTableVersionCallbackDelegateInterop>
    {
        public MposGetTableVersionToInterop(MposGetTableVersionCallbackDelegate callback) : base(callback) { }

        protected override MposGetTableVersionCallbackDelegateInterop convert => (mpos, error, version) =>
        {
            releaseGC();

            var cleanVersionBytes = PtrHelper.DerefList<byte>(version, 10).ToArray();
            var cleanVersion = Mpos.GetString(cleanVersionBytes);
            return callback(mpos, error, cleanVersion);
        };
    }
}
