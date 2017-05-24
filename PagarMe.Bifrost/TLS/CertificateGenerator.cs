using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost
{
    class CertificateGenerator
    {
        internal CertificateGenerator(String algorithm, Int32 validYears, Int32 keyStrength)
        {
            this.algorithm = algorithm;
            this.validYears = validYears;
            this.keyStrength = keyStrength;
        }

        private String algorithm;
        private Int32 validYears;
        private Int32 keyStrength;

        internal ComposedCertificate Generate(Boolean setPrivate, String subjectName, String issuerName, AsymmetricKeyParameter parentPrivate)
        {
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);

            var generator = new X509V3CertificateGenerator();

            setSerialNumber(generator, random);
            setSubjectAndIssuer(generator, subjectName, issuerName);
            setValidity(generator);

            var keyPair = setPublicKey(generator, random);

            var x509 = signAndMerge(generator, random, parentPrivate ?? keyPair.Private);

            if (setPrivate)
                setPrivateKey(x509, keyPair);

            return new ComposedCertificate(x509, keyPair.Private);
        }

        private static void setSerialNumber(X509V3CertificateGenerator generator, SecureRandom random)
        {
            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            generator.SetSerialNumber(serialNumber);
        }

        private static void setSubjectAndIssuer(X509V3CertificateGenerator generator, String subjectName, String issuerName)
        {
            var subjectDN = new X509Name(subjectName);
            generator.SetSubjectDN(subjectDN);

            var issuerDN = issuerName == null ? subjectDN : new X509Name(issuerName);
            generator.SetIssuerDN(issuerDN);

            var subjectAlt = subjectName.Replace("CN=", "");
            var subjectAltName = new GeneralNames(new GeneralName(GeneralName.DnsName, subjectAlt));
            generator.AddExtension(X509Extensions.SubjectAlternativeName, false, subjectAltName);
        }

        private void setValidity(X509V3CertificateGenerator generator)
        {
            var notBefore = DateTime.UtcNow.Date;
            var notAfter = notBefore.AddYears(validYears);
            generator.SetNotBefore(notBefore);
            generator.SetNotAfter(notAfter);
        }

        private AsymmetricCipherKeyPair setPublicKey(X509V3CertificateGenerator generator, SecureRandom random)
        {
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            var keyPair = keyPairGenerator.GenerateKeyPair();

            generator.SetPublicKey(keyPair.Public);
            return keyPair;
        }

        private X509Certificate2 signAndMerge(X509V3CertificateGenerator generator, SecureRandom random, AsymmetricKeyParameter keyParameter)
        {
            // Signature Algorithm
            generator.SetSignatureAlgorithm(algorithm);

            // selfsign certificate
            var certificate = generator.Generate(keyParameter, random);

            // merge into X509Certificate2
            return new X509Certificate2(certificate.GetEncoded());
        }

        private void setPrivateKey(X509Certificate2 x509, AsymmetricCipherKeyPair keyPair)
        {
            // correcponding private key
            var info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);

            var seq = (Asn1Sequence)Asn1Object.FromByteArray(info.PrivateKey.GetDerEncoded());
            if (seq.Count != 9)
                throw new PemException("malformed sequence in RSA private key");

            var rsa = new RsaPrivateKeyStructure(seq);
            var rsaparams = new RsaPrivateCrtKeyParameters(
                rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent,
                rsa.Prime1, rsa.Prime2,
                rsa.Exponent1, rsa.Exponent2,
                rsa.Coefficient
            );

            x509.PrivateKey = DotNetUtilities.ToRSA(rsaparams);
        }

    }

}