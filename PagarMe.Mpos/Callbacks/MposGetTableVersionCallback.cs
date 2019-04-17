using System;
using System.Linq;
using System.Threading.Tasks;
using PagarMe.Mpos.Entities;
using PagarMe.Mpos.Natives;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos.Callbacks
{
    class MposGetTableVersionCallback
    {
        public static MposGetTableVersionCallbackDelegate Callback(Mpos mpos, MposTablesLoadedCallbackDelegate tableCallback, int amount, PaymentMethod magstripePaymentMethod, TaskCompletionSource<PaymentResult> source)
        {
            return GCHelper.ManualFree<MposGetTableVersionCallbackDelegate>(releaseGC =>
            {
                return (mposPtr, err, version) =>
                {
                    releaseGC();
                    return callback(mpos, tableCallback, version);
                };
            });
        }

        private static Error callback(Mpos mpos, MposTablesLoadedCallbackDelegate tableCallback, String version)
        {
            if (!mpos.TMSStorage.GetGlobalVersion().StartsWith(version))
            {
                var aidEntries = mpos.TMSStorage.GetAidRows();
                var capkEntries = mpos.TMSStorage.GetCapkRows();

                var aidList = aidEntries.Select(a => new Aid(a)).ToArray();
                var capkList = capkEntries.Select(c => new Capk(c)).ToArray();
                var updateError = UpdateTables(mpos, tableCallback, aidList, capkList);

                if (updateError != Error.Ok)
                    throw new MposException(updateError);
            }
            else
            {
                tableCallback(mpos.nativeMpos, 0, false);
            }

            return Error.Ok;
        }

    }
}
