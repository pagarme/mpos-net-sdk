using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace PagarMe.Mpos
{	
	public class ApplicationEntry {
		public int PaymentMethod { get; set; }
		public string CardBrand { get; set; }

		public int AcquirerNumber { get; set; }
		public int RecordNumber { get; set; }
	}

	public class AcquirerEntry {
		public int Number { get; set; }
		public int CryptographyMethod { get; set; }
		public int KeyIndex { get; set; }

		public string SessionKey { get; set; }
		public string EmvTags { get; set; }
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
	
	public class TMSStorage
	{
		private SQLiteConnection db;	

		public TMSStorage(string path, string encryptionKey) {
			db = new SQLiteConnection(path + "pagarme_mpos.sqlite");
			db.CreateTable<ApplicationEntry>();
			db.CreateTable<AcquirerEntry>();
			db.CreateTable<RiskManagementEntry>();
		}

		public void StoreAcquirerRow(int number, int cryptographyMethod, int keyIndex, byte[] sessionKey, int emvTagsLength, int[] emvTags) {
			var acquirer = db.Table<AcquirerEntry>().Where(e => (e.Number == number)).FirstOrDefault();
			if (acquirer != null) {
				db.Delete(acquirer);
			}

			Console.WriteLine("[Storage] SessionKey = " + System.Text.Encoding.UTF8.GetString(sessionKey));
			Console.WriteLine("[Srtorage] EmvTags = " + String.Join(",", emvTags));

			AcquirerEntry entry = new AcquirerEntry {
				Number = number,
				CryptographyMethod = cryptographyMethod,
				KeyIndex = keyIndex,
				SessionKey = System.Text.Encoding.UTF8.GetString(sessionKey),
				EmvTags = String.Join(",", emvTags)
			};
			db.Insert(entry);
		}

		public void StoreRiskManagementRow(int acquirerNumber, int recordNumber, bool mustRiskManagement, int floorLimit, int brsPercentage, int brsThreshold, int brsMaxPercentage) {
			var profile = db.Table<RiskManagementEntry>().Where(e => (e.AcquirerNumber == acquirerNumber && e.RecordNumber == recordNumber)).FirstOrDefault();
			if (profile != null) {
				db.Delete(profile);
			}
			
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

		public void StoreApplicationRow(int paymentMethod, string cardBrand, int acquirerNumber, int recordNumber) {
			var application = SelectApplication(cardBrand, paymentMethod);
			if (application != null) {
				db.Delete(application);
			}
			
			ApplicationEntry e = new ApplicationEntry {
				PaymentMethod = paymentMethod,
				CardBrand = cardBrand,
				AcquirerNumber = acquirerNumber,
				RecordNumber = recordNumber
			};
			db.Insert(e);
		}		

		public AcquirerEntry[] GetAcquirerRows() {
			return db.Table<AcquirerEntry>().ToArray();
		}

		public RiskManagementEntry[] GetRiskManagementRows() {
			return db.Table<RiskManagementEntry>().ToArray();
		}

		public ApplicationEntry SelectApplication(string brand, int paymentMethod) {
			var query = db.Table<ApplicationEntry>().Where(e => (e.PaymentMethod == paymentMethod && e.CardBrand == brand));
			
			return query.FirstOrDefault();
		}
	}
}

