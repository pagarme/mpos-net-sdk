using Org.BouncyCastle.Utilities.IO.Pem;
using PagarMe.Bifrost.Certificates.Generation;
using PagarMe.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost.Certificates.Stores
{
    class UnixStore : Store
    {
        public override X509Certificate2 GetCertificate(String subject, String issuer, StoreName storeName)
        {
            return logger.TryLogOnException(() =>
            {
                X509Certificate2 certificate = null;
                return certificate;
            });
        }

        private const String storePath = "/usr/local/share/ca-certificates";

        public override void AddCertificate(X509Certificate2 ca, X509Certificate2 tls)
        {
            logger.TryLogOnException(() =>
            {
                addCertficate(ca, tls);
            });
        }

        private static void addCertficate(params X509Certificate2[] certificateList)
        {
            foreach (var certificate in certificateList)
            {
                createFile(certificate);
            }

            var assemblyInfo = new FileInfo(Assembly.GetEntryAssembly().Location);
            var storeScriptPath = Path.Combine(assemblyInfo.Directory.FullName, "certificates-unix-store.sh");
            var info = new FileInfo(storeScriptPath);

            var exitCode = run("sh", storeScriptPath, storePath, TLSConfig.Address);

            if (exitCode != 0)
            {
                throw new Exception($"Could not install certificate: bash exited with code {exitCode}");
            }
        }

        private static String createFile(X509Certificate2 certificate)
        {
            var certFileName = certificate.Subject.CleanSubject();
            var certPath = Path.Combine(storePath, $"{certFileName}.crt");

            using (var stream = new FileStream(certPath, FileMode.Create))
            using (var textWriter = new StreamWriter(stream))
            {
                var pemWriter = new PemWriter(textWriter);

                var pemObj = new PemObject("CERTIFICATE", certificate.RawData);
                pemWriter.WriteObject(pemObj);

                pemWriter.Writer.Flush();
                pemWriter.Writer.Close();
            }

            return certPath;
        }

        private static Int32 run(String command, params String[] args)
        {
            var joinedArgs = String.Join(" ", args);

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo(command, joinedArgs)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                },
                EnableRaisingEvents = true,
            };

            proc.Start();
            proc.WaitForExit();

            return proc.ExitCode;
        }
    }


}