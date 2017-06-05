using PagarMe.Bifrost.Certificates.Generation;
using PagarMe.Generic;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost.Certificates.Stores
{
    class WindowsStore : Store
    {
        private static StoreLocation storeLocation = StoreLocation.LocalMachine;

        public override X509Certificate2 GetCertificate(String subject, String issuer, StoreName storeName)
        {
            return logger.TryLogOnException(() =>
            {
                var store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadOnly);

                var collection = store.Certificates
                    .Find(X509FindType.FindBySubjectDistinguishedName, subject, false);

                if (!String.IsNullOrEmpty(issuer))
                {
                    collection = collection
                        .Find(X509FindType.FindByIssuerDistinguishedName, issuer, false);
                }

                var certificate = collection.Cast<X509Certificate2>().FirstOrDefault();

                store.Close();

                return certificate;
            });
        }

        public override void AddCertificate(X509Certificate2 ca, X509Certificate2 tls)
        {
            logger.TryLogOnException(() =>
            {
                addToFirefox(tls);

                addCertificate(ca, StoreName.Root);
                addCertificate(tls, StoreName.My);
            });
        }

        private static void addCertificate(X509Certificate2 certificate, StoreName storeName)
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);

            store.Close();
        }

        private void addToFirefox(X509Certificate2 tls)
        {
            logger.TryLogOnException(() =>
            {
                var storePath = logger.GetLogDirectoryPath();
                var certPath = Path.Combine(storePath, $"{TLSConfig.Address}.crt");
                var bytes = tls.Export(X509ContentType.Cert);
                File.WriteAllBytes(certPath, bytes);

                var storeScriptPath = "certificates-windows-firefox-store.bat";
                var info = new FileInfo(storeScriptPath);

                var filename = "MSVCR71.DLL";
                var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
                var destination = Path.Combine(systemPath, filename);

                if (!File.Exists(destination))
                {
                    var msvcr71Path = Path.Combine(Terminal.AssemblyPath, "WindowsFirefox", "program", filename);
                    File.Copy(msvcr71Path, destination);
                }

                logger.Info(Terminal.AssemblyPath);
                logger.Info(storeScriptPath);
                logger.Info(storePath);
                logger.Info(Path.Combine(storePath, $"{TLSConfig.Address}.crt"));

                var exitCode = Terminal.Run(storeScriptPath, storePath, TLSConfig.Address);

                if (exitCode != 0)
                {
                    throw new Exception($"Could not install certificate at Firefox: exited with code {exitCode}");
                }
            });
        }
    }


}