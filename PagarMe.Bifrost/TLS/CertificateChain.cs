using System;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost
{
    internal class CertificateChain
    {
        private CertificateGenerator certGen;

        public CertificateChain(String algorithm, Int32 validYears, Int32 keyStrength)
        {
            certGen = new CertificateGenerator(algorithm, validYears, keyStrength);
        }

        public X509Certificate2 GetOrGenerate(String subjectTls, String subjectCa)
        {
            var certificate = Store.GetCertificate(subjectTls, subjectCa, StoreName.My);

            if (certificate == null)
            {
                var ca = getOrGenerate(StoreName.Root, false, subjectCa);
                var tls = getOrGenerate(StoreName.My, true, subjectTls, ca);
                certificate = tls.X509;
            }

            return certificate;
        }

        private ComposedCertificate getOrGenerate(StoreName storeName, Boolean setPrivate, String subject, ComposedCertificate parent = null)
        {
            var issuer = parent?.X509.Subject;

            var certificate = certGen.Generate(setPrivate, subject, issuer, parent?.Private);

            Store.AddCertificate(certificate.X509, storeName);

            return certificate;
        }

    }



}