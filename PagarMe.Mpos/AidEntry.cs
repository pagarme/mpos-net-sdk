using System;
using Newtonsoft.Json;

namespace PagarMe.Mpos
{
    public struct AidEntry
    {
        // Entry
        [JsonProperty("acquirer_number")]
        public int AcquirerNumber { get; set; }
        [JsonProperty("record_index")]
        public int RecordIndex { get; set; }

        // Application
        [JsonProperty("aid")]
        public string Aid { get; set; }
        [JsonProperty("application_type")]
        public int ApplicationType { get; set; }
        [JsonProperty("application_name")]
        public string ApplicationName { get; set; }

        // Region/Currency
        [JsonProperty("country_code")]
        public int CountryCode { get; set; }
        [JsonProperty("currency")]
        public int Currency { get; set; }
        [JsonProperty("currency_exponent")]
        public int CurrencyExponent { get; set; }

        // Terminal
        [JsonProperty("terminal_capabilities")]
        public string TerminalCapabilities { get; set; }
        [JsonProperty("additional_terminal_capabilities")]
        public string AdditionalTerminalCapabilities { get; set; }
        [JsonProperty("terminal_type")]
        public int TerminalType { get; set; }

        // Terminal Action Codes
        [JsonProperty("default_tac")]
        public string DefaultTac { get; set; }
        [JsonProperty("denial_tac")]
        public string DenialTac { get; set; }
        [JsonProperty("online_tac")]
        public string OnlineTac { get; set; }

        // Limits
        [JsonProperty("floor_limit")]
        public int FloorLimit { get; set; }

        // Contactless
        [JsonProperty("contactless_zero_online_only")]
        public bool ContactlessZeroOnlineOnly { get; set; }
        [JsonProperty("contactless_mode")]
        public int ContactlessMode { get; set; }
        [JsonProperty("contactless_transaction_limit")]
        public int ContactlessTransactionLimit { get; set; }
        [JsonProperty("contactless_floor_limit")]
        public int ContactlessFloorLimit { get; set; }
        [JsonProperty("contactless_cvm_limit")]
        public int ContactlessCvmLimit { get; set; }
        [JsonProperty("contactless_application_version")]
        public int ContactlessApplicationVersion { get; set; }

        // Object Lists
        [JsonProperty("tdol")]
        public string Tdol;
        [JsonProperty("ddol")]
        public string Ddol;
    }
}

