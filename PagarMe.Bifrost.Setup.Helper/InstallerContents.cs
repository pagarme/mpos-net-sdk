using PagarMe.Generic;
using System;
using System.IO;
using System.IO.Compression;

namespace PagarMe.Bifrost.Setup.Helper
{
    internal class InstallerContents : ProgramVersion
    {
        public static void PublishLinux()
        {
            var currentVersion = GetCurrentVersion();

            var updatesPath = $@"{MainDir}PagarMe.Bifrost.Updates\";

            makeLinuxZip(currentVersion, updatesPath);

            var jsonPath = $"{updatesPath}update-linux.json";
            var json = $@"{{ ""last_version_name"": ""{currentVersion}"" }}";
            File.WriteAllText(jsonPath, json);
        }

        private static void makeLinuxZip(String currentVersion, String updatesPath)
        {
            var originFiles = $@"{MainDir}bin\Debug\Linux";

            var msi = Path.Combine(originFiles, "windows.msi");
            File.Delete(msi);
            var exe = Path.Combine(originFiles, "setup.exe");
            File.Delete(exe);

            var zipDestination = Path.Combine(updatesPath, $"bifrost-installer-{currentVersion}.zip");
            FileExtension.DeleteIfExists(zipDestination);
            ZipFile.CreateFromDirectory(originFiles, zipDestination, CompressionLevel.Optimal, false);
        }

        public static void PublishWindows()
        {
            var currentVersion = GetCurrentVersion();

            var updatesPath = $@"{MainDir}PagarMe.Bifrost.Updates\";

            var originMsi = $@"{MainDir}bin\Debug\Windows\BifrostInstaller.msi";
            var msiDestination = $"{updatesPath}bifrost-installer-{currentVersion}.msi";
            File.Copy(originMsi, msiDestination, true);

            var jsonPath = $"{updatesPath}update-windows.json";
            var json = $@"{{ ""last_version_name"": ""{currentVersion}"" }}";
            File.WriteAllText(jsonPath, json);
        }
    }
}