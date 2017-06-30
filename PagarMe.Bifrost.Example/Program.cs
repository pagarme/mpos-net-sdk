using System;
using System.Threading;

namespace PagarMe.Bifrost.Example
{
    internal static class Program
    {
        private static Options options;

        private static int Main(string[] args)
        {
            options = Options.Get(args);

            if (!options.ParsedSuccessfully)
                return 1;

            var bridge = new MposBridge(options);

            var running = true;
            Console.CancelKeyPress += (sender, e) => running = false;

            bridge.Start(false);

            while (running)
                Thread.Yield();

            bridge.Stop();

            return 0;
        }

    }
}