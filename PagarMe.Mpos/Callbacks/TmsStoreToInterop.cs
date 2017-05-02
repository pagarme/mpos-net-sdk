using PagarMe.Mpos.Helpers;
using System.Collections.Generic;
using static PagarMe.Mpos.Mpos.Native;

namespace PagarMe.Mpos.Callbacks
{
    internal class TmsStoreToInterop : GCEnvelop<TmsStoreCallbackDelegate, TmsStoreCallbackDelegateInterop>
    {
        public TmsStoreToInterop(TmsStoreCallbackDelegate callback) : base(callback) { }

        protected override TmsStoreCallbackDelegateInterop convert => (version, tables, tableLen, applications, appLen, riskManagement, riskmanLen, acquirers, acqLen, userData) =>
        {
            releaseGC();
            var capkList = new List<Capk>();
            var aidList = new List<Aid>();

            for (var i = 0; i < tableLen; i++)
            {
                // We assume everything is the smaller member
                var capk = PtrHelper.DoubleDeref<Capk>(tables, i);
                var isAid = capk.IsAid;

                if (isAid)
                {
                    var aid = PtrHelper.DoubleDeref<Aid>(tables, i);
                    aidList.Add(aid);
                }
                else
                {
                    capkList.Add(capk);
                }
            }

            var appList = PtrHelper.DoubleDerefList<Application>(applications, appLen);
            var riskProfileList = PtrHelper.DoubleDerefList<RiskManagement>(riskManagement, riskmanLen);
            var acquirerList = PtrHelper.DoubleDerefList<Acquirer>(acquirers, acqLen);

            return callback(version, capkList, aidList, appList, riskProfileList, acquirerList, userData);
        };
    }
}
