using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

namespace PagarMe.Mpos
{
    internal static class ApiHelper
    {
        static ApiHelper()
        {
            ApiEndpoint = "https://api.pagar.me/1";
            //ApiEndpoint = "http://192.168.64.2:3000";
        }

        public static string ApiEndpoint { get; set; }

        private static HttpWebRequest CreateRequest(string method, string path, string auth)
        {
            var request = WebRequest.Create(ApiEndpoint + path);

            request.Method = method;

            if (auth != null)
                request.Headers.Add("Authorization",
                    "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(auth + ":x")));

            return (HttpWebRequest) request;
        }

        private static async Task<Tuple<string, string>> GetCardHashKey(string encryptionKey)
        {
            var response =
                (HttpWebResponse)
                await CreateRequest("GET", "/transactions/card_hash_key", encryptionKey).GetResponseAsync();

            var json = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            var result = JsonConvert.DeserializeObject<dynamic>(json);

            return new Tuple<string, string>(result.id.ToString(), result.public_key.ToString());
        }

        private static byte[] Combine(byte[][] arrays)
        {
            var offset = 0;
            var result = new byte[arrays.Sum(a => a.Length)];

            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        private static byte[] Encrypt(byte[] data, AsymmetricKeyParameter key)
        {
            var output = new List<byte[]>();
            var engine = new Pkcs1Encoding(new RsaEngine());

            engine.Init(true, key);

            var blockSize = engine.GetInputBlockSize();

            for (var chunkPosition = 0; chunkPosition < data.Length; chunkPosition += blockSize)
            {
                var chunkSize = Math.Min(blockSize, data.Length - chunkPosition);
                output.Add(engine.ProcessBlock(data, chunkPosition, chunkSize));
            }

            return Combine(output.ToArray());
        }

        private static string EncryptWith(string id, string publicKey, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);

            using (var reader = new StringReader(publicKey))
            {
                var pemReader = new PemReader(reader);
                var key = (AsymmetricKeyParameter) pemReader.ReadObject();

                return id + "_" + Convert.ToBase64String(Encrypt(bytes, key));
            }
        }

        public static async Task<string> CreateCardHash(string encryptionKey, string data)
        {
            var hashParameters = await GetCardHashKey(encryptionKey);

            return EncryptWith(hashParameters.Item1, hashParameters.Item2, data);
        }

        public static async Task<string> GetTerminalTables(string encryptionKey, string checksum, int[] dukptKeys)
        {
            var request = CreateRequest("GET",
                "/terminal/updates?checksum=" + WebUtility.UrlEncode(checksum) + "&dukpt_keys=[" +
                string.Join(",", dukptKeys) + "]", encryptionKey);
            try
            {
                var response = (HttpWebResponse) await request.GetResponseAsync();
                return new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError &&
                    ((HttpWebResponse) ex.Response).StatusCode == HttpStatusCode.NotModified)
                    return "";

                throw;
            }
        }
    }
}