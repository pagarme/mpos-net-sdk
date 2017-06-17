using PagarMe.Generic;
using System;
using System.IO;
using System.Reflection;

namespace PagarMe.Bifrost.Updates
{
    class WindowsUpdater : Updater
    {
        // Temporary address, will be defined when distribution is decided
        private const String Address = "http://localhost:2001";

        private RequestMaker request = new RequestMaker(Address);
        private Version currentVersion = Assembly.GetEntryAssembly().GetName().Version;
        private String downloadedFile;
        private String productCode;

        protected override Boolean? Check()
        {
            var checkResult = request.GetObjectFromUrl<UpdateInfo>().WaitResult();
            var newVersion = checkResult.LastVersion;
            if (currentVersion == newVersion) return false;

            var filename = $"bifrost-installer-{newVersion}.msi";
            downloadedFile = Path.Combine(logger.GetLogDirectoryPath(), filename);

            var downloaded = 
                request.DownloadBinaryFromUrl(downloadedFile, filename)
                       .WaitResult();

            if (!downloaded) return null;

            productCode = Product.GetCode(downloadedFile);
            if (productCode == null) return null;

            return true;
        }

        protected override Boolean Update()
        {
            lock (this)
            {
                var succededUninstall = runOnMsi($"/x {productCode}", "uninstall");
                if (!succededUninstall) return false;

                var succededInstall = runOnMsi($"/i {downloadedFile}", "install");
                if (!succededInstall) return false;

                return true;
            }
        }

        private bool runOnMsi(String command, String description)
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

        class UpdateInfo
        {
            public String LastVersionName { get; set; }
            public Version LastVersion => new Version(LastVersionName);
        }
    }
}
