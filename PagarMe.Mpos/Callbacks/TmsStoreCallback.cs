using PagarMe.Mpos.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PagarMe.Mpos.Mpos;

namespace PagarMe.Mpos.Callbacks
{
    class TmsStoreCallback
    {
        private IList<Native.Capk> capkList;
        private IList<Native.Aid> aidList;
        private IList<Native.Application> appList;
        private IList<Native.RiskManagement> riskProfileList;
        private IList<Native.Acquirer> acquirerList;

        public static Native.TmsStoreCallbackDelegate Callback(Mpos mpos, bool forceUpdate, TaskCompletionSource<bool> source)
        {
            return GCHelper.ManualFree<Native.TmsStoreCallbackDelegate>(releaseGC =>
            {
                return (version, capkList, aidList, appList, riskProfileList, acquirerList, userData) =>
                {
                    releaseGC();

                    var instance = new TmsStoreCallback()
                    {
                        capkList = capkList,
                        aidList = aidList,
                        appList = appList,
                        riskProfileList = riskProfileList,
                        acquirerList = acquirerList
                    };

                    return instance.insertDataIntoStorage(mpos, forceUpdate, source, version);
                };
            });
        }

        private Native.Error insertDataIntoStorage(Mpos mpos, bool forceUpdate, 
                TaskCompletionSource<bool> source, string version)
        {
            var callback = MposTablesLoadedSynchronizeTablesCallback.Callback(mpos, source);

            mpos.TMSStorage.PurgeIndex();
            mpos.TMSStorage.StoreGlobalVersion(version);

            foreach (var capk in capkList)
            {
                mpos.TMSStorage.StoreCapkRow(capk.AcquirerNumber, capk.RecordIndex, capk.Rid, capk.CapkIndex,
                          capk.ExponentLength, capk.Exponent, capk.ModulusLength, capk.Modulus,
                          capk.HasChecksum, capk.Checksum);
            }

            foreach (var aid in aidList)
            {
                mpos.TMSStorage.StoreAidRow(aid.AcquirerNumber, aid.RecordIndex, aid.AidLength, aid.AidNumber,
                        aid.ApplicationType, aid.ApplicationNameLength, aid.ApplicationName, aid.AppVersion1,
                        aid.AppVersion2, aid.AppVersion3, aid.CountryCode, aid.Currency,
                        aid.CurrencyExponent, aid.MerchantId, aid.Mcc, aid.TerminalId,
                        aid.TerminalCapabilities, aid.AdditionalTerminalCapabilities, aid.TerminalType,
                        aid.DefaultTac, aid.DenialTac, aid.OnlineTac, aid.FloorLimit, aid.Tcc,
                        aid.CtlsZeroAm, aid.CtlsMode, aid.CtlsTransactionLimit, aid.CtlsFloorLimit,
                        aid.CtlsCvmLimit, aid.CtlsApplicationVersion, aid.TdolLength, aid.Tdol,
                        aid.DdolLength, aid.Ddol);
            }

            foreach (var app in appList)
            {
                mpos.TMSStorage.StoreApplicationRow(app.PaymentMethod, app.CardBrand, app.AcquirerNumber,
                    app.RecordNumber, app.EmvTagsLength, app.EmvTags);
            }

            foreach (var profile in riskProfileList)
            {
                mpos.TMSStorage.StoreRiskManagementRow(profile.AcquirerNumber, profile.RecordNumber,
                    profile.MustRiskManagement, profile.FloorLimit, profile.BiasedRandomSelectionPercentage,
                    profile.BiasedRandomSelectionThreshold, profile.BiasedRandomSelectionMaxPercentage);
            }

            foreach (var acquirer in acquirerList)
            {
                mpos.TMSStorage.StoreAcquirerRow(acquirer.Number, acquirer.CryptographyMethod, acquirer.KeyIndex,
                    acquirer.SessionKey);
            }

            var updateError = Native.UpdateTables(mpos, callback, aidList.ToArray(), capkList.ToArray());

            if (updateError != Native.Error.Ok)
                throw new MposException(updateError);

            return updateError;
        }


    }
}
