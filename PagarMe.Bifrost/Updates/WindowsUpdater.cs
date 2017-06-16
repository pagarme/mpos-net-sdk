using NLog;
using PagarMe.Generic;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace PagarMe.Bifrost.Updates
{
    public class WindowsUpdater
    {
        // Temporary address, will be defined when distribution is decided
        private const String Address = "http://localhost:2001";

        private static RequestMaker request = new RequestMaker(Address);
        private static Version currentVersion = Assembly.GetEntryAssembly().GetName().Version;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public async static Task<Boolean> CheckAndUpdate()
        {
            logger.Info("Update check starting");

            var updated = false;

            try
            {
                updated = await checkAndUpdate();
            }
            catch (Exception e)
            {
                logger.Error(e);
            }

            return updated;
        }

        private async static Task<Boolean> checkAndUpdate()
        {
            var update = await request.GetObjectFromUrl<Update>();
            var shouldUpgradeOrDowngrade = currentVersion != update.LastVersion;

            if (!shouldUpgradeOrDowngrade)
            {
                logger.Info($"Bifrost up to date: {currentVersion}");
                return false;
            }

            var filename = $"bifrost-installer-{update.LastVersion}.msi";
            var filepath = Path.Combine(logger.GetLogDirectoryPath(), filename);
            var downloaded = await request.DownloadBinaryFromUrl(filepath, filename);

            if (!downloaded) return false;

            return await Task.Run(() =>
            {
                return reinstall(filepath, update.LastVersion);
            });
        }

        private static Boolean reinstall(String filepath, Version newVersion)
        {
            var code = Product.GetCode(filepath);
            if (code == null) return false;

            var succededUninstall = runOnMsi($"/x {code}", "uninstall");
            if (!succededUninstall) return false;

            var succededInstall = runOnMsi($"/i {filepath}", "install");
            if (!succededInstall) return false;

            logger.Info($"Finished upgrading.");
            logger.Info($"New version: {newVersion}.");

            return true;
        }

        private static bool runOnMsi(String command, String description)
        {
            logger.Info($"Starting {description}");
            var result = Terminal.Run("msiexec", command, "/quiet");

            if (!result.Succedded)
            {
                logger.Warn(result.Output);
                logger.Warn(result.Error);
                logger.Warn($"Error: {description} process exited with code {result.Code}");
            }

            return result.Succedded;
        }

        class Update
        {
            public String LastVersionName { get; set; }
            public Version LastVersion => new Version(LastVersionName);
        }
    }
}
