using System;
using System.IO;
using CommandLine;

namespace PagarMe.Bifrost
{
    public class Options
    {
        [Option('p', "port", Required = false, HelpText = "Port to bind the server", DefaultValue = 2000)]
        public int BindPort { get; set; }

        [Option('b', "bind", Required = false, HelpText = "Address to bind the server", DefaultValue = "localhost")]
        public string BindAddress { get; set; }

        [Option('e', "endpoint", Required = false, HelpText = "Pagar.me's API endpoint",
            DefaultValue = "https://api.pagar.me/1/")]
        public string Endpoint { get; set; }

        [Option('d', "data-path", Required = false, HelpText = "Database path", DefaultValue = "<appdata>")]
        public string DataPath { get; set; }

        public void EnsureDefaults()
        {
            if (DataPath == null || DataPath == "<appdata>")
                DataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PagarMe.Bifrost"
                );
        }
    }
}