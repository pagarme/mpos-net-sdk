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
                    Help.Print();
                    break;

                case "-u":
                case "--update-setup-version":
                    runByOS(args, SetupVersion.UpdateLinux, SetupVersion.UpdateWindows);
                    break;

                case "-p":
                case "--publish-msi-json":
                    runByOS(args, InstallerContents.PublishLinux, InstallerContents.PublishWindows);
                    break;

                default:
                    throw new ArgumentException($"Unrecognized argument {args[0]}. See help (-h).");
            }
        }

        private static void runByOS(String[] args, Action linux, Action windows)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException($"OS required. See help (-h).");
            }

            switch (args[1]?.ToLower())
            {
                case "-l":
                case "--linux":
                    linux();
                    break;

                case "-w":
                case "--windows":
                    windows();
                    break;

                default:
                    throw new ArgumentException($"Unrecognized argument {args[1]}. See help (-h).");
            }
        }
    }
}
