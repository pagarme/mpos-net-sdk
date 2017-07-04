using PagarMe.Mpos.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using static PagarMe.Mpos.Mpos;

namespace PagarMe.Mpos.Callbacks
{
    class MposGetTableVersionCallback
    {
        public static Native.MposGetTableVersionCallbackDelegate Callback(Mpos mpos, Native.MposTablesLoadedCallbackDelegate tableCallback, int amount, PaymentMethod magstripePaymentMethod, TaskCompletionSource<PaymentResult> source)
        {
            return GCHelper.ManualFree<Native.MposGetTableVersionCallbackDelegate>(releaseGC =>
            {
                return (mposPtr, err, version) =>
                {
                    releaseGC();
                    return callback(mpos, tableCallback, version);
                };
            });
        }

        private static Native.Error callback(Mpos mpos, Native.MposTablesLoadedCallbackDelegate tableCallback, String version)
        {
            if (!mpos.TMSStorage.GetGlobalVersion().StartsWith(version))
            {
                var aidEntries = mpos.TMSStorage.GetAidRows();
                var capkEntries = mpos.TMSStorage.GetCapkRows();

                var aidList = aidEntries.Select(a => new Native.Aid(a)).ToArray();
                var capkList = capkEntries.Select(c => new Native.Capk(c)).ToArray();
                var updateError = Native.UpdateTables(mpos, tableCallback, aidList, capkList);

                if (updateError != Native.Error.Ok)
                    throw new MposException(updateError);
            }
            else
            {
                tableCallback(mpos.nativeMpos, 0, false);
            }

            return Native.Error.Ok;
        }

    }
}
