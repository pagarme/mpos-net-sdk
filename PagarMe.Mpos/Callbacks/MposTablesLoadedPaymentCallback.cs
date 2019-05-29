using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PagarMe.Mpos.Entities;
using PagarMe.Mpos.Natives;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos.Callbacks
{
    class MposTablesLoadedPaymentCallback
    {
        public static MposTablesLoadedCallbackDelegate Callback(Mpos mpos, int amount, IEnumerable<EmvApplication> applications, PaymentMethod magstripePaymentMethod, TaskCompletionSource<PaymentResult> source)
        {
            return GCHelper.ManualFree<MposTablesLoadedCallbackDelegate>(releaseGC =>
            {
                return (mposPtr, tableError, loaded) =>
                {
                    releaseGC();
                    return callback(mpos, amount, applications, magstripePaymentMethod, source);
                };

            });
        }

        private static Error callback(Mpos mpos, int amount, IEnumerable<EmvApplication> applications, PaymentMethod magstripePaymentMethod, TaskCompletionSource<PaymentResult> source)
        {
            var callback = MposPaymentCallback.Callback(mpos, source);

            var acquirers = new List<Acquirer>();
            var riskProfiles = new List<RiskManagement>();

            var rawApplications = new List<Application>();
            if (applications != null)
                foreach (var application in applications)
                {
                    var entries = mpos.TMSStorage.SelectApplication(application.Brand, (int)application.PaymentMethod);

                    entries.ToList().ForEach(a =>
	                    {
		                    rawApplications.Add(new Application(a));
	                    }
                    );
                }
            else
                foreach (var entry in mpos.TMSStorage.GetApplicationRows())
                    rawApplications.Add(new Application(entry));

            foreach (var entry in mpos.TMSStorage.GetAcquirerRows()) acquirers.Add(new Acquirer(entry));

            foreach (var entry in mpos.TMSStorage.GetRiskManagementRows())
                riskProfiles.Add(new RiskManagement(entry));

            var error = ProcessPayment(mpos.nativeMpos, amount, rawApplications.ToArray(), rawApplications.Count,
                acquirers.ToArray(), acquirers.Count, riskProfiles.ToArray(), riskProfiles.Count,
                (int)magstripePaymentMethod, callback);

            if (error != Error.Ok)
                throw new MposException(error);

            return Error.Ok;
        }
    }
}
