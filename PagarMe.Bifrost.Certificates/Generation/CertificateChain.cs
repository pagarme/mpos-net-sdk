using NLog;
using PagarMe.Bifrost.Certificates.Stores;
using System;
using System.Security.Cryptography.X509Certificates;
using certType = PagarMe.Bifrost.Certificates.Generation.CertificateGenerator.Type;

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

        public X509Certificate2 Generate(String subjectTls, String subjectCa)
        {
            logger.Info("Start generating certificates.");
            var ca = getOrGenerate(certType.Root, subjectCa);
            logger.Info("Root certificate generated.");
            var tls = getOrGenerate(certType.SSL, subjectTls, ca);
            logger.Info("Tls certificate generated.");

            Store.Instance.AddCertificate(ca.X509, tls.X509);

            return tls.X509;
        }

        private ComposedCertificate getOrGenerate(certType certType, String subject, ComposedCertificate parent = null)
        {
            return certGen.Generate(certType, subject, parent?.X509?.Subject, parent?.Private);
        }

    }



}