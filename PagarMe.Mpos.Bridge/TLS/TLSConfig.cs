using System;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Mpos.Bridge
{
    internal class TLSConfig
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

        internal static String Address;

        public static X509Certificate2 GetCertificate()
        {
            var certificateChain = new CertificateChain(algorithm, validYears, keyStrength);

            return certificateChain.GetOrGenerate(subjectTls, subjectCa);
        }
    }
}