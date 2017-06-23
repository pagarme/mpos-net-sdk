using PagarMe.Generic;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost.Certificates.Stores
{
    class WindowsStore : Store
    {
        private static StoreLocation storeLocation = StoreLocation.LocalMachine;

        public override X509Certificate2 GetCertificate(String subject, String issuer, StoreName storeName)
        {
            return Log.TryLogOnException(() =>
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
            Log.TryLogOnException(() =>
            {
                Log.Me.Info("Add certificates to Store");

                addToOS(ca, StoreName.Root);
                addToOS(tls, StoreName.My);

                WindowsFirefoxStore.AddToFirefox(ca, tls);
            });
        }

        private void addToOS(X509Certificate2 certificate, StoreName storeName)
        {
            Log.Me.Info($"Adding {certificate.Subject.CleanSubject()} to Windows Store {storeName}");

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
    }
}