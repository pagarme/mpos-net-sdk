using System;
using System.IO;
using System.Linq;

namespace PagarMe.Bifrost.Setup.Helper
{
    internal class Help
    {
        internal static void Print()
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
    }
}
