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
            WebRequest request = WebRequest.Create(ApiEndpoint + "/transactions/card_hash_key");

            request.Method = "GET";
            request.PreAuthenticate = true;
            request.Credentials = new NetworkCredential(encryptionKey, "x");

            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());

            string json = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            dynamic result = JsonConvert.DeserializeObject<dynamic>(json);

            return new Tuple<string, string>(result.id.ToString(), result.public_key.ToString());
        }

        static byte[] Combine( params byte[][] arrays )
        {
            byte[] rv = new byte[ arrays.Sum( a => a.Length ) ];
            int offset = 0;
            foreach ( byte[] array in arrays ) {
                System.Buffer.BlockCopy( array, 0, rv, offset, array.Length );
                offset += array.Length;
            }
            return rv;
        }

        static byte[] Encrypt(byte[] data, AsymmetricKeyParameter key)
        {
            IAsymmetricBlockCipher e = new Pkcs1Encoding(new RsaEngine());

            List<byte[]> output = new List<byte[]>();

            e.Init(true, key);

            int blockSize = e.GetInputBlockSize();

            for (int chunkPosition = 0; chunkPosition < data.Length; chunkPosition += blockSize)
            {
                int chunkSize = Math.Min(blockSize, data.Length - chunkPosition);
                output.Add(e.ProcessBlock(data, chunkPosition, chunkSize));
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

