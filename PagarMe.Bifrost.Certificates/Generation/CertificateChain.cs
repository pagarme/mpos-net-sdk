using NLog;
using PagarMe.Bifrost.Certificates.Stores;
using System;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost.Certificates.Generation
{
    internal class CertificateChain
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private CertificateGenerator certGen;

        public CertificateChain(String algorithm, Int32 validYears, Int32 keyStrength)
        {
            certGen = new CertificateGenerator(algorithm, validYears, keyStrength);
        }

        public X509Certificate2 Get(String subjectTls, String subjectCa)
        {
            return Store.Instance.GetCertificate(subjectTls, subjectCa, StoreName.My);
        }

        public X509Certificate2 GenerateIfNotExists(String subjectTls, String subjectCa)
        {
            var certificate = Get(subjectTls, subjectCa);

            if (certificate == null)
            {
                logger.Info("Certificate not found. Start generating.");
                var ca = getOrGenerate(false, subjectCa);
                logger.Info("Root certificate generated.");
                var tls = getOrGenerate(true, subjectTls, ca);
                logger.Info("Tls certificate generated.");

                Store.Instance.AddCertificate(ca.X509, tls.X509);

                certificate = tls.X509;
            }

            return certificate;
        }

        private ComposedCertificate getOrGenerate(Boolean setPrivate, String subject, ComposedCertificate parent = null)
        {
            var issuer = parent?.X509.Subject;

            var certificate = certGen.Generate(setPrivate, subject, issuer, parent?.Private);

            return certificate;
        }

    }



}