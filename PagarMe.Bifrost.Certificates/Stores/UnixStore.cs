using Org.BouncyCastle.Utilities.IO.Pem;
using PagarMe.Bifrost.Certificates.Generation;
using PagarMe.Generic;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost.Certificates.Stores
{
    class UnixStore : Store
    {
        private const String storePath = "/usr/local/share/ca-certificates";
        private static readonly String certName = TLSConfig.Address;
        private static readonly String pfxPath = Path.Combine(storePath, $"{certName}.pfx");

        public override X509Certificate2 GetCertificate(String subject, String issuer, StoreName storeName)
        {
            return logger.TryLogOnException(() =>
            {
                if (!File.Exists(pfxPath)) return null;
                return new X509Certificate2(pfxPath);
            });
        }

        public override void AddCertificate(X509Certificate2 ca, X509Certificate2 tls)
        {
            logger.TryLogOnException(() =>
            {
                addCertficate(tls);
                logger.Info("Finish generating");
            });
        }

        private static void addCertficate(X509Certificate2 certificate)
        {
            var crtPath = createCrt(certificate);
            var keyPath = createKey(certificate);

            var storeScriptPath = "certificates-unix-store-add.sh";
            var info = new FileInfo(storeScriptPath);

            var installResult = Terminal.Run("sh", storeScriptPath, storePath, certName);
            if (!installResult.Succedded)
            {
                logger.Error(installResult.Output);
                logger.Error(installResult.Error);
                throw new Exception($"Could not install certificate: bash exited with code {installResult}");
            }

            File.Delete(crtPath);
            File.Delete(keyPath);
        }

        private static String createCrt(X509Certificate2 certificate)
        {
            return createPemFile("CERTIFICATE", certificate.RawData, "crt");
        }

        public static String createKey(X509Certificate2 certificate)
        {
            return createPemFile("RSA PRIVATE KEY", certificate.GetPrivateKeyRawData(), "key");
        }

        private static String createPemFile(String title, Byte[] content, String extension)
        {
            var certPath = Path.Combine(storePath, $"{certName}.{extension}");

            using (var stream = new FileStream(certPath, FileMode.Create))
            using (var textWriter = new StreamWriter(stream))
            {
                var pemWriter = new PemWriter(textWriter);

                var pemObj = new PemObject(title, content);
                pemWriter.WriteObject(pemObj);

                pemWriter.Writer.Flush();
                pemWriter.Writer.Close();
            }

            return certPath;
        }
    }
}