using System;
using System.Threading;
using CommandLine;
using NLog;

namespace PagarMe.Bifrost.Server
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int Main(string[] args)
        {
            var running = true;
            var options = new Options();
            var isValid = Parser.Default.ParseArgumentsStrict(args, options);
            options.EnsureDefaults();

            if (!isValid)
                return 1;

            options.EnsureDefaults();

            var server = new MposBridge(options);

            Console.CancelKeyPress += (sender, e) => running = false;

            Logger.Info("mPOS Websocket Bridge");

            try
            {
                Logger.Info("Starting server");
                server.Start();
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "An error ocurred during startup");
                return 1;
            }

            while (running)
                Thread.Yield();

            Logger.Info("Stopping server");
            server.Stop();

            return 0;
        }
    }
}