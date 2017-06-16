﻿using PagarMe.Bifrost.Certificates.Generation;
using PagarMe.Generic;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace PagarMe.Bifrost.Certificates.Stores
{
    class WindowsStore : Store
    {
        private static StoreLocation storeLocation = StoreLocation.LocalMachine;

        public override X509Certificate2 GetCertificate(String subject, String issuer, StoreName storeName)
        {
            return logger.TryLogOnException(() =>
            {
                var store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadOnly);

                var collection = store.Certificates
                    .Find(X509FindType.FindBySubjectDistinguishedName, subject, false);

                if (!String.IsNullOrEmpty(issuer))
                {
                    collection = collection
                        .Find(X509FindType.FindByIssuerDistinguishedName, issuer, false);
                }

                var certificate = collection.Cast<X509Certificate2>().FirstOrDefault();

                store.Close();

                return certificate;
            });
        }

        public override void AddCertificate(X509Certificate2 ca, X509Certificate2 tls)
        {
            logger.TryLogOnException(() =>
            {
                addToOS(ca, StoreName.Root);
                addToOS(tls, StoreName.My);

                addToFirefox(tls);
            });
        }

        private void addToOS(X509Certificate2 certificate, StoreName storeName)
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);

            var oldCert = GetCertificate(certificate.Subject, certificate.Issuer, storeName);

            if (oldCert != null)
            {
                store.Remove(oldCert);
            }

            store.Add(certificate);

            store.Close();
        }

        private void addToFirefox(X509Certificate2 tls)
        {
            logger.TryLogOnException(() =>
            {
                var storePath = logger.GetLogDirectoryPath();

                exportCertificate(storePath, tls);

                copyMSVCR71();

                installOnFireFox(storePath);
            });
        }

        private static void exportCertificate(String storePath, X509Certificate2 tls)
        {
            var certPath = Path.Combine(storePath, $"{TLSConfig.Address}.crt");
            var bytes = tls.Export(X509ContentType.Cert);
            File.WriteAllBytes(certPath, bytes);
        }

        private static void copyMSVCR71()
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

        private static void installOnFireFox(String windowsStorePath)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var mozillaPath = Path.Combine(appData, "Mozilla");

            if (!Directory.Exists(mozillaPath)) return;

            var storeScriptPath = "certificates-windows-firefox-store.bat";

            var certDbs = Directory.GetFiles(mozillaPath, "*cert*.db", SearchOption.AllDirectories);
            var mozillaCertPath = Path.GetDirectoryName(certDbs.First());

            var installResult = Terminal.Run(storeScriptPath, mozillaCertPath, windowsStorePath, TLSConfig.Address);

            if (!installResult.Succedded)
            {
                logger.Error(installResult.Output);
                logger.Error(installResult.Error);
                throw new Exception($"Could not install certificate at Firefox: exited with code {installResult.Code}");
            }
        }
    }


}