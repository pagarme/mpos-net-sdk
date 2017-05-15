using NLog;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Mpos.Bridge
{
    class Store
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static X509Certificate2 GetCertificate(String subject, String issuer, StoreName storeName)
        {
            return logger.TryLogOnException(() =>
            {
                var store = new X509Store(storeName);
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

        public static void AddCertificate(X509Certificate2 cert, StoreName storeName)
        {
            logger.TryLogOnException(() =>
            {
                var store = new X509Store(storeName);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);

                store.Close();
            });
        }
    }



}