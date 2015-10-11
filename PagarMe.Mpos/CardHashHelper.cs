using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.OpenSsl;
using Newtonsoft.Json;

namespace PagarMe.Mpos
{
    public static class CardHashHelper
    {
        public static string ApiEndpoint { get; set; }

        static CardHashHelper()
        {
            ApiEndpoint = "http://localhost:3000";
        }

        static async Task<Tuple<string, string>> GetCardHashKey(string encryptionKey)
        {
            var request = WebRequest.Create(ApiEndpoint + "/transactions/card_hash_key");

            request.Method = "GET";
            request.PreAuthenticate = true;
            request.Credentials = new NetworkCredential(encryptionKey, "x");

            var response = (HttpWebResponse)(await request.GetResponseAsync());

            var json = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            var result = JsonConvert.DeserializeObject<dynamic>(json);

            return new Tuple<string, string>(result.id.ToString(), result.public_key.ToString());
        }

        static byte[] Combine(byte[][] arrays)
        {
            int offset = 0;
            byte[] result = new byte[arrays.Sum(a => a.Length)];

            foreach (byte[] array in arrays) {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        static byte[] Encrypt(byte[] data, AsymmetricKeyParameter key)
        {
            List<byte[]> output = new List<byte[]>();
            Pkcs1Encoding engine = new Pkcs1Encoding(new RsaEngine());

            engine.Init(true, key);

            int blockSize = engine.GetInputBlockSize();

            for (int chunkPosition = 0; chunkPosition < data.Length; chunkPosition += blockSize)
            {
                int chunkSize = Math.Min(blockSize, data.Length - chunkPosition);
                output.Add(engine.ProcessBlock(data, chunkPosition, chunkSize));
            }

            return Combine(output.ToArray());
        }

        static string EncryptWith(string id, string publicKey, string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            using (var reader = new StringReader(publicKey))
            {
                var pemReader = new PemReader(reader);
                var key = (AsymmetricKeyParameter)pemReader.ReadObject();

                return id + "_" + Convert.ToBase64String(Encrypt(bytes, key));
            }
        }

        public static async Task<string> CreateCardHash(string encryptionKey, string data)
        {
            Tuple<string, string> hashParameters = await GetCardHashKey(encryptionKey);

            return EncryptWith(hashParameters.Item1, hashParameters.Item2, data);
        }
    }
}

