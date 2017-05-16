using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost.Certificates.TLS
{
    public class TLSConfig
    {
        public static bool ClientValidate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        static String algorithm => "SHA256WithRSA";
        static Int32 validYears => 100;
        static Int32 keyStrength => 4096;
        static String subjectTls => "CN=" + Address;
        static String subjectCa => "CN=Bifrost";

        public static String Address;

        static CertificateChain certificateChain = new CertificateChain(algorithm, validYears, keyStrength);

        public static X509Certificate2 Get()
        {
            return certificateChain.Get(subjectTls, subjectCa);
        }

        internal static X509Certificate2 GenerateIfNotExists()
        {
            return certificateChain.GenerateIfNotExists(subjectTls, subjectCa);
        }

    }
}