using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;

namespace PagarMe.Bifrost.Certificates.TLS
{
    internal class ComposedCertificate
    {
        public X509Certificate2 X509 { get; private set; }
        public AsymmetricKeyParameter Private { get; private set; }

        public ComposedCertificate(X509Certificate2 x509, AsymmetricKeyParameter @private)
        {
            X509 = x509;
            Private = @private;
        }
    }
}