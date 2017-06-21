using System;
using System.Linq;
using System.Text;
using SQLite;

namespace PagarMe.Mpos
{	
	public class ApplicationEntry {
		public int PaymentMethod { get; set; }
		public string CardBrand { get; set; }

		public int AcquirerNumber { get; set; }
		public int RecordNumber { get; set; }
		
		public string EmvTags { get; set; }
	}

	public class AcquirerEntry {
		public int Number { get; set; }
		public int CryptographyMethod { get; set; }
		public int KeyIndex { get; set; }

		public string SessionKey { get; set; }
	}

	public class RiskManagementEntry {
		public int AcquirerNumber { get; set; }
		public int RecordNumber { get; set; }

		public bool MustRiskManagement { get; set; }
		public int FloorLimit { get; set; }
		public int BiasedRandomSelectionPercentage { get; set; }
		public int BiasedRandomSelectionThreshold { get; set; }
		public int BiasedRandomSelectionMaxPercentage { get; set; }
	}

	public class AidEntry {
		public int AcquirerNumber { get; set; }
		public int RecordIndex { get; set; }
		
		public string Aid { get; set; }
		public int ApplicationType { get; set; }
		public string ApplicationName { get; set; }
		public string AppVersion1 { get; set; }
		public string AppVersion2 { get; set; }
		public string AppVersion3 { get; set; }

		public int CountryCode { get; set; }
		public int Currency { get; set; }
		public int CurrencyExponent { get; set; }
		public string MerchantId { get; set; }
		public int Mcc { get; set; }
		public string TerminalId { get; set; }

		public string TerminalCapabilities { get; set; }
		public string AdditionalTerminalCapabilities { get; set; }
		public int TerminalType { get; set; }
		public string DefaultTac { get; set; }
		public string DenialTac { get; set; }
		public string OnlineTac { get; set; }
		public int FloorLimit { get; set; }
		public string Tcc { get; set; }

		public bool CtlsZeroAm { get; set; }
		public int CtlsMode { get; set; }
		public int CtlsTransactionLimit { get; set; }
		public int CtlsFloorLimit { get; set; }
		public int CtlsCvmLimit { get; set; }
		public string CtlsApplicationVersion { get; set; }

		public string Tdol { get; set; }
		public string Ddol { get; set; }
	}

	public class CapkEntry {
		public int AcquirerNumber { get; set; }
		public int RecordIndex { get; set; }

		public string Rid { get; set; }
		public int PublicKeyId { get; set; }
		public string Exponent { get; set; }
		public string Modulus { get; set; }
		public string Checksum { get; set; }
	}

	public class GlobalVersionEntry {
		public string Version { get; set; }
	}
	
	public class TMSStorage
	{
		private SQLiteConnection db;	

		public TMSStorage(string path, string encryptionKey) {
			db = new SQLiteConnection(path + "pagarme_mpos.sqlite");
			db.CreateTable<ApplicationEntry>();
			db.CreateTable<AcquirerEntry>();
			db.CreateTable<RiskManagementEntry>();
			db.CreateTable<GlobalVersionEntry>();
			db.CreateTable<AidEntry>();
			db.CreateTable<CapkEntry>();
		}

		public void PurgeIndex() {
			db.DeleteAll<ApplicationEntry>();
			db.DeleteAll<AcquirerEntry>();
			db.DeleteAll<RiskManagementEntry>();
			db.DeleteAll<GlobalVersionEntry>();
			db.DeleteAll<AidEntry>();
			db.DeleteAll<CapkEntry>();
		}
		
		public void StoreGlobalVersion(string version) {
			db.Insert(new GlobalVersionEntry { Version = version });
		}

		public void StoreAcquirerRow(int number, int cryptographyMethod, int keyIndex, byte[] sessionKey) {
			AcquirerEntry entry = new AcquirerEntry {
				Number = number,
				CryptographyMethod = cryptographyMethod,
				KeyIndex = keyIndex,
				SessionKey = Encoding.ASCII.GetString(sessionKey, 0, 32),
			};
			db.Insert(entry);
		}

		public void StoreRiskManagementRow(int acquirerNumber, int recordNumber, bool mustRiskManagement, int floorLimit, int brsPercentage, int brsThreshold, int brsMaxPercentage) {
			RiskManagementEntry entry = new RiskManagementEntry {
				AcquirerNumber = acquirerNumber,
				RecordNumber = recordNumber,
				MustRiskManagement = mustRiskManagement,
				FloorLimit = floorLimit,
				BiasedRandomSelectionPercentage = brsPercentage,
				BiasedRandomSelectionThreshold = brsThreshold,
				BiasedRandomSelectionMaxPercentage = brsMaxPercentage				
			};
			db.Insert(entry);			
		}

		public void StoreApplicationRow(int paymentMethod, string cardBrand, int acquirerNumber, int recordNumber, int emvTagsLength, int[] emvTags) {
			int[] cleanEmvTags = new int[emvTagsLength];
			for (int i = 0; i < emvTagsLength; i++) {
				cleanEmvTags[i] = emvTags[i];
			}

			ApplicationEntry entry = new ApplicationEntry {
				PaymentMethod = paymentMethod,
				CardBrand = cardBrand,
				AcquirerNumber = acquirerNumber,
				RecordNumber = recordNumber,
				EmvTags = String.Join(",", cleanEmvTags)
			};
			db.Insert(entry);
		}

		public void StoreAidRow(int acqNumber, int recNumber, int aidLen, byte[] aid, int appType, int appNameLength, byte[] appName, byte[] appVer1, byte[] appVer2, byte[] appVer3, int country, int currency, int exponent, byte[] merchantId, int mcc, byte[] terminalId, byte[] capabilities, byte[] additionalCapabilities, int type, byte[] tacDefault, byte[] tacDenial, byte[] tacOnline, int floorLimit, byte tcc, bool ctlsZeroAm, int ctlsMode, int crlsTrxLimit, int ctlsFloorLimit, int ctlsCvmLimit, byte[] ctlsAppVer, int tdolLen, byte[] tdol, int ddolLen, byte[] ddol) {
			AidEntry entry = new AidEntry {
				AcquirerNumber = acqNumber,
				RecordIndex = recNumber,
				
				Aid = Encoding.ASCII.GetString(aid, 0, aidLen * 2),
				ApplicationType = appType,
				ApplicationName = Encoding.ASCII.GetString(appName, 0, appNameLength),
				AppVersion1 = Encoding.ASCII.GetString(appVer1, 0, 4),
				AppVersion2 = Encoding.ASCII.GetString(appVer2, 0, 4),
				AppVersion3 = Encoding.ASCII.GetString(appVer3, 0, 4),

				CountryCode = country,
				Currency = currency,
				CurrencyExponent = exponent,
				MerchantId = Encoding.ASCII.GetString(merchantId, 0, 15),
				Mcc = mcc,
				TerminalId = Encoding.ASCII.GetString(terminalId, 0, 8),

				TerminalCapabilities = Encoding.ASCII.GetString(capabilities, 0, 6),
				AdditionalTerminalCapabilities = Encoding.ASCII.GetString(additionalCapabilities, 0, 10),
				TerminalType = type,
				DefaultTac = Encoding.ASCII.GetString(tacDefault, 0, 10),
				DenialTac = Encoding.ASCII.GetString(tacDenial, 0, 10),
				OnlineTac = Encoding.ASCII.GetString(tacOnline, 0, 10),
				FloorLimit = floorLimit,
				Tcc = Convert.ToString(tcc),

				CtlsZeroAm = ctlsZeroAm,
				CtlsMode = ctlsMode,
				CtlsTransactionLimit = crlsTrxLimit,
				CtlsFloorLimit = ctlsFloorLimit,
				CtlsCvmLimit = ctlsCvmLimit,
				CtlsApplicationVersion = Encoding.ASCII.GetString(ctlsAppVer, 0, 4),

				Tdol = Encoding.ASCII.GetString(tdol, 0, tdolLen),
				Ddol = Encoding.ASCII.GetString(ddol, 0, ddolLen)		
			};
			db.Insert(entry);
		}

		public void StoreCapkRow(int acqNumber, int recNumber, byte[] rid, int capkIndex, int expLen, byte[] exp, int modLen, byte[] mod, bool hasChecksum, byte[] checksum) {
			CapkEntry entry = new CapkEntry {
				AcquirerNumber = acqNumber,
				RecordIndex = recNumber,

				Rid = Encoding.ASCII.GetString(rid, 0, 10),
				PublicKeyId = capkIndex,
				Exponent = Encoding.ASCII.GetString(exp, 0, expLen * 2),
				Modulus = Encoding.ASCII.GetString(mod, 0, modLen * 2),
				Checksum = hasChecksum ? Encoding.ASCII.GetString(checksum, 0, 40) : null
			};
			db.Insert(entry);
		}

		public string GetGlobalVersion() {
			GlobalVersionEntry e = db.Table<GlobalVersionEntry>().FirstOrDefault();
			if (e != null) return e.Version;
			return "";
		}
		
		public AcquirerEntry[] GetAcquirerRows() {
			return db.Table<AcquirerEntry>().ToArray();
		}

		public RiskManagementEntry[] GetRiskManagementRows() {
			return db.Table<RiskManagementEntry>().ToArray();
		}

		public AidEntry[] GetAidRows() {
			return db.Table<AidEntry>().ToArray();
		}
		
		public CapkEntry[] GetCapkRows() {
			return db.Table<CapkEntry>().ToArray();
		}		

		public ApplicationEntry SelectApplication(string brand, int paymentMethod) {
			var query = db.Table<ApplicationEntry>().Where(e => (e.PaymentMethod == paymentMethod && e.CardBrand == brand));
			return query.FirstOrDefault();
		}

		public ApplicationEntry[] GetApplicationRows() {
			return db.Table<ApplicationEntry>().ToArray();
		}
	}
}

