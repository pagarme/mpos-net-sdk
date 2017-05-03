using System;
using PagarMe.Mpos.Bridge;
using PagarMe.Generic;
using CommandLine;
using System.Threading;

namespace QuickTest
{
    internal static class Program
    {
        private static Options options;

        private static int Main(string[] args)
        {
            options = new Options()
            {
                BaudRate = Config.BaudRate,
                EncryptionKey = Config.EncryptionKey
            };

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