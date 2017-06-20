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
                return getCertificate(subject, issuer, storeName);
            });
        }

        private static X509Certificate2 getCertificate(String subject, String issuer, StoreName storeName)
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
        }

        public override void AddCertificate(X509Certificate2 ca, X509Certificate2 tls)
        {
            logger.TryLogOnException(() =>
            {
                logger.Info("Add certificates to Store");

                addToOS(ca, StoreName.Root);
                addToOS(tls, StoreName.My);

                addChainToFirefox(ca, tls);
            });
        }

        private void addToOS(X509Certificate2 certificate, StoreName storeName)
        {
            logger.Info($"Adding {certificate.Subject.CleanSubject()} to Windows Store {storeName}");

            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);

            var oldCert = getCertificate(certificate.Subject, certificate.Issuer, storeName);

            if (oldCert != null)
            {
                store.Remove(oldCert);
            }

            store.Add(certificate);

            store.Close();
        }

        private void addChainToFirefox(X509Certificate2 ca, X509Certificate2 tls)
        {
            copyCertutilHelperLib();

            addToFirefox(ca, "TC,,");
            addToFirefox(tls, "u,,");
        }

        private static void copyCertutilHelperLib()
        {
            var filename = "MSVCR71.DLL";
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var destination = Path.Combine(systemPath, filename);

            if (!File.Exists(destination))
            {
                var msvcr71Path = Path.Combine(Terminal.AssemblyPath, filename);
                File.Copy(msvcr71Path, destination);
                logger.Info($"{filename} copied to {systemPath}");
            }
        }

        private void addToFirefox(X509Certificate2 cert, String trust)
        {
            var storePath = logger.GetLogDirectoryPath();

            try
            {
                exportCertificate(storePath, cert);
                installOnFireFox(storePath, cert, trust);
            }
            finally
            {
                deleteExported(storePath, cert);
            }
        }

        private static void exportCertificate(String storePath, X509Certificate2 cert)
        {
            var subject = cert.Subject.CleanSubject();
            var certPath = Path.Combine(storePath, $"{subject}.crt");
            var bytes = cert.Export(X509ContentType.Cert);
            File.WriteAllBytes(certPath, bytes);
        }

        private static void installOnFireFox(String windowsStorePath, X509Certificate2 cert, String trust)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var mozillaPath = Path.Combine(appData, "Mozilla");

            if (!Directory.Exists(mozillaPath)) return;

            var storeScriptPath = "certificates-windows-firefox-store.bat";

            var certDbs = Directory.GetFiles(mozillaPath, "*cert*.db", SearchOption.AllDirectories);
            foreach(var certDb in certDbs)
            {
                logger.Info($"Adding {cert.Subject.CleanSubject()} to {certDb}");
                var mozillaCertPath = Path.GetDirectoryName(certDb);

                var subject = cert.Subject.CleanSubject();
                var installResult = Terminal.Run(storeScriptPath, mozillaCertPath, windowsStorePath, subject, trust);
                if (!installResult.Succedded)
                {
                    logger.Error(installResult.Output);
                    logger.Error(installResult.Error);
                    throw new Exception($"Could not install certificate at Firefox: exited with code {installResult.Code}");
                }
            }
        }

        private static void deleteExported(String storePath, X509Certificate2 cert)
        {
            var subject = cert.Subject.CleanSubject();
            var certPath = Path.Combine(storePath, $"{subject}.crt");

            if (File.Exists(certPath)) File.Delete(certPath);
        }
    }


}