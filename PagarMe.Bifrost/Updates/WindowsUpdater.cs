using PagarMe.Generic;
using System;
using System.IO;

namespace PagarMe.Bifrost.Updates
{
    class WindowsUpdater : Updater
    {
        private String downloadedFile;
        private String productCode;

        protected override Boolean? Check()
        {
            var request = new RequestMaker(UpdateAddress);

            var checkResult = 
                request.GetObjectFromUrl<UpdateInfo>
                    ("update-windows.json")
                    .WaitResult();

            var newVersion = checkResult.LastVersion.ToString(3);

            if (ProgramEnvironment.CurrentVersion == newVersion) return false;

            var filename = $"bifrost-installer-{newVersion}.msi";
            downloadedFile = Path.Combine(Log.GetLogDirectoryPath(), filename);

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
            Log.Me.Info($"Starting {description}");
            var result = Terminal.Run("msiexec", command, "/quiet");

            if (!result.Succedded)
            {
                Log.Me.Warn(result.Output);
                Log.Me.Warn(result.Error);
                Log.Me.Warn($"Error: {description} process exited with code {result.Code}");
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
