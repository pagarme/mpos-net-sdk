using System.IO;
using System.Text.RegularExpressions;

namespace PagarMe.Bifrost.Setup.Helper
{
    internal class SetupVersion : ProgramVersion
    {
        internal static void UpdateWindows()
        {
            update("Windows");
        }

        internal static void UpdateLinux()
        {
            update("Linux");
        }

        private static void update(string os)
        {
            var currentVersion = GetCurrentVersion();

            var projName = $"PagarMe.Bifrost.{os}";
            var projPath = $@"{MainDir}{projName}\{projName}.vdproj";
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
    }
}