using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PagarMe.Bifrost.Setup.Helper
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) throw new ArgumentNullException("Missing arguments");

            switch(args[0])
            {
                case "-h":
                case "--help":
                    printHelp();
                    break;

                case "-u":
                case "--update-setup-version":
                    updateSetupVersion();
                    break;

                case "-p":
                case "--publish-msi-json":
                    publishMsiAndJson();
                    break;

                default:
                    throw new ArgumentException($"Unrecognized argument {args[0]}. See help (-h).");
            }
        }

        private const String mainDir = @"..\..\..\";

        private static void printHelp()
        {
            var helpFilePath = "help-setup-helper.txt";

            if (!File.Exists(helpFilePath))
            {
                Console.WriteLine("Help file missing!");
                return;
            }

            File.ReadAllLines(helpFilePath)
                .ToList()
                .ForEach(Console.WriteLine);
        }

        private static void updateSetupVersion()
        {
            var currentVersion = getCurrentVersion();

            var projName = "PagarMe.Bifrost.Windows";
            var projPath = $@"{mainDir}{projName}\{projName}.vdproj";
            var projContent = File.ReadAllLines(projPath);
            var rewrite = false;
            
            for (var l = 0; l < projContent.Length; l++)
            {
                if (projContent[l].Contains(@"""ProductVersion"""))
                {
                    var pattern = @"(\d+\.\d+\.\d+)";
                    var newLine = Regex.Replace(projContent[l], pattern, currentVersion);

                    if (projContent[l] != newLine)
                    {
                        projContent[l] = newLine;
                        rewrite = true;
                    }
                }
            }

            if (rewrite)
            {
                File.WriteAllLines(projPath, projContent);
            }
        }

        private static void publishMsiAndJson()
        {
            var currentVersion = getCurrentVersion();

            var updatesPath = $@"{mainDir}PagarMe.Bifrost.Updates\";

            var originMsi = $@"{mainDir}bin\Debug\Windows\BifrostInstaller.msi";
            var msiDestination = $"{updatesPath}bifrost-installer-{currentVersion}.msi";
            File.Copy(originMsi, msiDestination, true);

            var jsonPath = $"{updatesPath}update.json";
            var json = $@"{{ ""last_version_name"": ""{currentVersion}"" }}";
            File.WriteAllText(jsonPath, json);
        }

        private static String getCurrentVersion()
        {
            var assemblyInfoPath = $@"{mainDir}PagarMe.Bifrost\Properties\BifrostAssemblyInfo.cs";
            var assemblyInfoContent = File.ReadAllText(assemblyInfoPath);

            var regexVersion = new Regex(@"AssemblyVersion\(""(\d+.\d+.\d+)");
            var version = regexVersion.Match(assemblyInfoContent).Groups[1].Value;

            return version;
        }
    }
}
