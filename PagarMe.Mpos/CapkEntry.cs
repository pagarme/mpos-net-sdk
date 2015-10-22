using System;
using Newtonsoft.Json;

namespace PagarMe.Mpos
{
    public struct CapkEntry
    {
        // Entry
        [JsonProperty("acquirer_number")]
        public int AcquirerNumber { get; set; }
        [JsonProperty("record_index")]
        public int RecordIndex { get; set; }

        // Application Provider
        [JsonProperty("rid")]
        public string Rid { get; set; }

        // Public Key
        [JsonProperty("public_key_id")]
        public int PublicKeyId { get; set; }
        [JsonProperty("exponent")]
        public string Exponent { get; set; }
        [JsonProperty("modulus")]
        public string Modulus { get; set; }

        // Checksum
        [JsonProperty("checksum")]
        public string Checksum { get; set; }
    }
}

