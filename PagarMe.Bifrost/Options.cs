using System;
using System.IO;
using CommandLine;
using PagarMe.Generic;

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

        [Option('u', "update-address", Required = false, HelpText = "Address check for updates", DefaultValue = "http://localhost:2001")]
        public string UpdateAddress { get; set; }
        
        public Boolean ParsedSuccessfully { get; private set; }

        private Options() { }

        public static Options Get(String[] args)
        {
            var options = new Options();

            options.ParsedSuccessfully = Parser.Default.ParseArgumentsStrict(args, options);

            if (!options.ParsedSuccessfully)
            {
                Log.Me.Warn("Could not get parameters. Verify parameters passed.");
            }

            options.EnsureDefaults();

            return options;
        }

        private void EnsureDefaults()
        {
            if (DataPath == null || DataPath == "<appdata>")
                DataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PagarMe.Bifrost"
                );
        }
    }
}