using PagarMe.Mpos.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PagarMe.Mpos.Mpos;

namespace PagarMe.Mpos.Callbacks
{
    class MposTablesLoadedPaymentCallback
    {
        public static Native.MposTablesLoadedCallbackDelegate Callback(Mpos mpos, int amount, IEnumerable<EmvApplication> applications, PaymentMethod magstripePaymentMethod, TaskCompletionSource<PaymentResult> source)
        {
            return GCHelper.ManualFree<Native.MposTablesLoadedCallbackDelegate>(releaseGC =>
            {
                return (mposPtr, tableError, loaded) =>
                {
                    releaseGC();
                    return callback(mpos, amount, applications, magstripePaymentMethod, source);
                };

            });
        }

        private static Native.Error callback(Mpos mpos, int amount, IEnumerable<EmvApplication> applications, PaymentMethod magstripePaymentMethod, TaskCompletionSource<PaymentResult> source)
        {
            var callback = MposPaymentCallback.Callback(mpos, source);

            var acquirers = new List<Native.Acquirer>();
            var riskProfiles = new List<Native.RiskManagement>();

            var rawApplications = new List<Native.Application>();
            if (applications != null)
                foreach (var application in applications)
                {
                    var entry = mpos.TMSStorage.SelectApplication(application.Brand, (int)application.PaymentMethod);
                    if (entry != null) rawApplications.Add(new Native.Application(entry));
                }
            else
                foreach (var entry in mpos.TMSStorage.GetApplicationRows())
                    rawApplications.Add(new Native.Application(entry));

            foreach (var entry in mpos.TMSStorage.GetAcquirerRows()) acquirers.Add(new Native.Acquirer(entry));

            foreach (var entry in mpos.TMSStorage.GetRiskManagementRows())
                riskProfiles.Add(new Native.RiskManagement(entry));

            var error = Native.ProcessPayment(mpos.nativeMpos, amount, rawApplications.ToArray(), rawApplications.Count,
                acquirers.ToArray(), acquirers.Count, riskProfiles.ToArray(), riskProfiles.Count,
                (int)magstripePaymentMethod, callback);

            if (error != Native.Error.Ok)
                throw new MposException(error);

            return Native.Error.Ok;
        }
    }
}
