using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost.Certificates.Stores
{
    public static class X509Certificate2Extension
    {
        public static Byte[] GetPrivateKeyRawData(this X509Certificate2 certificate)
        {
            var privateKey = (RSACryptoServiceProvider)certificate.PrivateKey;

            var parameters = privateKey.ExportParameters(true);

            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE

                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    innerWriter.writeBigEndian(new byte[] { 0x00 }); // Version
                    innerWriter.writeBigEndian(parameters.Modulus);
                    innerWriter.writeBigEndian(parameters.Exponent);
                    innerWriter.writeBigEndian(parameters.D);
                    innerWriter.writeBigEndian(parameters.P);
                    innerWriter.writeBigEndian(parameters.Q);
                    innerWriter.writeBigEndian(parameters.DP);
                    innerWriter.writeBigEndian(parameters.DQ);
                    innerWriter.writeBigEndian(parameters.InverseQ);

                    var length = (int)innerStream.Length;
                    writer.writeLength(length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                return stream.GetBuffer();
            }
        }

        private static void writeBigEndian(this BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // INTEGER

            var prefixZeros = 0;

            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }

            if (value.Length - prefixZeros == 0)
            {
                stream.writeLength(1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    stream.writeLength(value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    stream.writeLength(value.Length - prefixZeros);
                }

                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }

        private static void writeLength(this BinaryWriter stream, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");

            if (length < 0x80)
            {
                // Short form
                stream.Write((byte)length);
            }
            else
            {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }

                stream.Write((byte)(bytesRequired | 0x80));

                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xff));
                }
            }
        }
    }

}