using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;

namespace PagarMe.Mpos
{	
	public class TMSStorage
	{
		private string key;
		private string path;

		public struct ApplicationEntry {
			public int PaymentMethod;
			public string CardBrand;

			public int AcquirerNumber;
			public int RecordNumber;
		}

		public struct AcquirerEntry {
			public int Number;
			public int CryptographyMethod;
			public int KeyIndex;

			public string SessionKey;
			public int[] EmvTags;
		}

		public struct RiskManagementEntry {
			public int AcquirerNumber;
			public int RecordNumber;

			public bool MustRiskManagement;
			public int FloorLimit;
			public int BiasedRandomSelectionPercentage;
			public int BiasedRandomSelectionThreshold;
			public int BiasedRandomSelectionMaxPercentage;
		}
		
		public TMSStorage(string path_, string encryptionKey) {
			path = path_;
			key = encryptionKey;
		}

		public void StoreAcquirerRow(int number, int cryptographyMethod, int keyIndex, byte[] sessionKey, int emvTagsLength, int[] emvTags) {
			using (var connection = new SQLiteConnection("Data Source=" + path + "pagarme_mpos.sqlite;Version=3;")) {
				SQLiteCommand create = new SQLiteCommand("CREATE TABLE IF NOT EXISTS acquirers (number INTEGER PRIMARY KEY ASC, cryptography_method INTEGER, key_index INTEGER, session_key VARCHAR(32), emv_tags VARCHAR(600))", connection);
				create.ExecuteNonQuery();

				string values = String.Format("{0},{1},{2},'{3}','{4}'", number, cryptographyMethod, keyIndex, System.Text.Encoding.Default.GetString(sessionKey), String.Join(",", emvTags));
				SQLiteCommand insert = new SQLiteCommand("INSERT OR REPLACE INTO acquirers (number, cryptography_method, key_index, session_key, emv_tags) VALUES (" + values + ")", connection);
				insert.ExecuteNonQuery();
			}
		}

		public void StoreRiskManagementRow(int acquirerNumber, int recordNumber, bool mustRiskManagement, int floorLimit, int brsPercentage, int brsThreshold, int brsMaxPercentage) {
			using (var connection = new SQLiteConnection("Data Source=" + path + "pagarme_mpos.sqlite;Version=3;")) {
				SQLiteCommand create = new SQLiteCommand("CREATE TABLE IF NOT EXISTS riskmanagement (acquirer_number INTEGER, record_number INTEGER, must_risk_management INTEGER, floor_limit INTEGER, brs_percentage INTEGER, brs_threshold INTEGER, brs_max_percentage INTEGER)", connection);
				create.ExecuteNonQuery();

				string values = String.Format("{0},{1},{2},{3},{4},{5},{6}", acquirerNumber, recordNumber, mustRiskManagement, floorLimit, brsPercentage, brsThreshold, brsMaxPercentage);
				SQLiteCommand insert = new SQLiteCommand("INSERT OR REPLACE INTO riskmanagement (acquirer_number, record_number, must_risk_management, floor_limit, brs_percentage, brs_threshold, brs_max_percentage) VALUES (" + values + ")", connection);
				insert.ExecuteNonQuery();
			}
		}

		public void StoreApplicationRow(int paymentMethod, string cardBrand, int acquirerNumber, int recordNumber) {
			using (var connection = new SQLiteConnection("Data Source=" + path + "pagarme_mpos.sqlite;Version=3;")) {
				SQLiteCommand create = new SQLiteCommand("CREATE TABLE IF NOT EXISTS applications (acquirer_number INTEGER, record_number INTEGER, payment_method INTEGER, card_brand VARCHAR(255))", connection);
				create.ExecuteNonQuery();

				string values = String.Format("{0},{1},{2},'{3}'", acquirerNumber, recordNumber, paymentMethod, cardBrand);
				SQLiteCommand insert = new SQLiteCommand("INSERT OR REPLACE INTO applications (acquirer_number, record_number, payment_method, card_brand) VALUES (" + values + ")", connection);
				insert.ExecuteNonQuery();
			}
		}		

		public AcquirerEntry[] GetAcquirerRows() {
			List<AcquirerEntry> entries = new List<AcquirerEntry>();
			using (var connection = new SQLiteConnection("Data Source=" + path + "pagarme_mpos.sqlite;Version=3;")) {			
				SQLiteCommand query = new SQLiteCommand("SELECT * FROM acquirers", connection);
				SQLiteDataReader reader = query.ExecuteReader();
				while (reader.Read()) {
					AcquirerEntry entry;
					
					entry.Number = Convert.ToInt32(reader["number"]);
					entry.CryptographyMethod = Convert.ToInt32(reader["cryptography_method"]);
					entry.KeyIndex = Convert.ToInt32(reader["key_index"]);
					entry.SessionKey = Convert.ToString(reader["session_key"]);
					entry.EmvTags = Convert.ToString(reader["emv_tags"]).Split(',').Select(int.Parse).ToArray();

					entries.Add(entry);
				}
			}
			
			return entries.ToArray();			
		}

		public RiskManagementEntry[] GetRiskManagementRows() {
			List<RiskManagementEntry> entries = new List<RiskManagementEntry>();
			using (var connection = new SQLiteConnection("Data Source=" + path + "pagarme_mpos.sqlite;Version=3;")) {			
				SQLiteCommand query = new SQLiteCommand("SELECT * FROM riskmanagement", connection);
				SQLiteDataReader reader = query.ExecuteReader();
				while (reader.Read()) {
					RiskManagementEntry entry;
					
					entry.AcquirerNumber = Convert.ToInt32(reader["acquirer_number"]);
					entry.RecordNumber = Convert.ToInt32(reader["record_number"]);
					entry.MustRiskManagement = Convert.ToBoolean(reader["must_risk_management"]);
					entry.FloorLimit = Convert.ToInt32(reader["floor_limit"]);
					entry.BiasedRandomSelectionPercentage = Convert.ToInt32(reader["brs_percentage"]);
					entry.BiasedRandomSelectionThreshold = Convert.ToInt32(reader["brs_threshold"]);
					entry.BiasedRandomSelectionMaxPercentage = Convert.ToInt32(reader["brs_max_percentage"]);

					entries.Add(entry);
				}
			}
			
			return entries.ToArray();
		}

		public ApplicationEntry? SelectApplication(string brand, int paymentMethod) {
			using (var connection = new SQLiteConnection("Data Source=" + path + "pagarme_mpos.sqlite;Version=3;")) {			
				SQLiteCommand query = new SQLiteCommand("SELECT * FROM applications WHERE card_brand='" + brand + "' AND payment_method=" + paymentMethod.ToString(), connection);
				SQLiteDataReader reader = query.ExecuteReader();
				if (reader.Read()) {
					ApplicationEntry entry;
					entry.AcquirerNumber = Convert.ToInt32(reader["acquirer_number"]);
					entry.RecordNumber = Convert.ToInt32(reader["record_number"]);
					entry.PaymentMethod = Convert.ToInt32(reader["payment_method"]);
					entry.CardBrand = Convert.ToString(reader["card_brand"]);

					return entry;
				}

				return null;
			}			
		}
	}
}

