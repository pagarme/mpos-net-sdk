using System;
using PagarMe.Bifrost;
using CommandLine;
using System.Threading;

namespace PagarMe.Bifrost.Example
{
    internal static class Program
    {
        private static Options options;

        private static int Main(string[] args)
        {
            options = new Options();
            var isValid = Parser.Default.ParseArgumentsStrict(args, options);
            options.EnsureDefaults();

            if (!isValid)
                return 1;

            var bridge = new MposBridge(options);

            var running = true;
            Console.CancelKeyPress += (sender, e) => running = false;

            bridge.Start();

            while (running)
                Thread.Yield();

            bridge.Stop();

            return 0;
        }

    }
}