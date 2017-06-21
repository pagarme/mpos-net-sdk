using PagarMe.Generic;
using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost.Certificates.Generation
{
    public class TLSConfig
    {
        public static bool ClientValidate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        static String algorithm => "SHA256WithRSA";
        static Int32 validYears => 100;
        static Int32 keyStrength => 2048;
        static String subjectTls => "CN=" + Address;
        static String subjectCa => "CN=Bifrost";

        public static String Address;

        static CertificateChain certificateChain = new CertificateChain(algorithm, validYears, keyStrength);

        public static X509Certificate2 Get()
        {
            var certificate = certificateChain.Get(subjectTls, subjectCa);

            if (ProgramEnvironment.IsUnix && certificate == null)
            {
                certificate = Generate();
            }

            return certificate;
        }

        internal static X509Certificate2 Generate()
        {
            return certificateChain.Generate(subjectTls, subjectCa);
        }

    }
}