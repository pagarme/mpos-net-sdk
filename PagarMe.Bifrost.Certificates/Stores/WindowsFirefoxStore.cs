using NLog;
using PagarMe.Bifrost.Certificates.Generation;
using PagarMe.Generic;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost.Certificates.Stores
{
    class WindowsFirefoxStore
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly String dataPath = logger.GetLogDirectoryPath();

        public static void AddToFirefox(X509Certificate2 ca, X509Certificate2 tls)
        {
            var mozillaPath = getMozillaPath();
            if (mozillaPath == null) return;

            copyCertutilHelperLib();

            addToFirefox(mozillaPath, ca, "TC,,");
            addToFirefox(mozillaPath, tls, "u,,");
        }

        private static String getMozillaPath()
        {
            // Cached used because auto-updater will not use right path
            // - the user in which firefox is installed
            var mozillaPathCache = Path.Combine(dataPath, "mozilla-store.path");

            if (File.Exists(mozillaPathCache))
            {
                var mozillaCachedPath = File.ReadAllText(mozillaPathCache);
                if (Directory.Exists(mozillaCachedPath)) return mozillaCachedPath;
                logger.Warn($"Data path {mozillaCachedPath} not found");
            }

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var mozillaPath = Path.Combine(appData, "Mozilla");

            if (Directory.Exists(mozillaPath))
            {
                ServiceUser.GrantAccess(mozillaPath);
                File.WriteAllText(mozillaPathCache, mozillaPath);
                return mozillaPath;
            }

            logger.Warn($"Data path {mozillaPath} not found");
            logger.Warn($"Skipped installing certificates at Firefox");
            return null;
        }

        private static void copyCertutilHelperLib()
        {
            var filename = "MSVCR71.DLL";
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var destination = Path.Combine(systemPath, filename);

            if (!File.Exists(destination))
            {
                var msvcr71Path = Path.Combine(Terminal.AssemblyPath, filename);
                File.Copy(msvcr71Path, destination);
                logger.Info($"{filename} copied to {systemPath}");
            }
        }

        private static void addToFirefox(String mozillaPath, X509Certificate2 cert, String trust)
        {
            try
            {
                exportCertificate(cert);
                installOnFireFox(mozillaPath, cert, trust);
            }
            finally
            {
                deleteExported(cert);
            }
        }

        private static void exportCertificate(X509Certificate2 cert)
        {
            var subject = cert.Subject.CleanSubject();
            var certPath = Path.Combine(dataPath, $"{subject}.crt");
            var bytes = cert.Export(X509ContentType.Cert);
            File.WriteAllBytes(certPath, bytes);
        }

        private static void installOnFireFox(String mozillaPath, X509Certificate2 cert, String trust)
        {
            var addScriptPath = "certificates-windows-firefox-store.bat";
            var subject = cert.Subject.CleanSubject();

            var certDbs = Directory.GetFiles(mozillaPath, "*cert*.db", SearchOption.AllDirectories);
            foreach (var certDb in certDbs)
            {
                logger.Info($"Adding {cert.Subject.CleanSubject()} to {certDb}");

                var mozillaCertPath = Path.GetDirectoryName(certDb);
                var installResult = Terminal.Run(addScriptPath, mozillaCertPath, dataPath, subject, trust);
                if (!installResult.Succedded)
                {
                    logger.Error(installResult.Output);
                    logger.Error(installResult.Error);
                    throw new Exception($"Could not install certificate at Firefox: exited with code {installResult.Code}");
                }
            }
        }

        private static void deleteExported(X509Certificate2 cert)
        {
            var subject = cert.Subject.CleanSubject();
            var certPath = Path.Combine(dataPath, $"{subject}.crt");

            if (File.Exists(certPath)) File.Delete(certPath);
        }
    }
}