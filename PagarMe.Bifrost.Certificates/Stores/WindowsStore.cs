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
    }


}