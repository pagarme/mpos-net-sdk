using System.ServiceProcess;
using CommandLine;
using NLog;
using System;
using PagarMe.Generic;

namespace PagarMe.Bifrost.Service
{
    public partial class MposWebsocketService : ServiceBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private MposBridge _bridge;

        public MposWebsocketService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.TryLogOnException(() =>
            {
                var options = new Options();
                var isValid = Parser.Default.ParseArgumentsStrict(args, options);
                options.EnsureDefaults();

                _bridge = new MposBridge(options);

                Logger.Info("mPOS Websocket Bridge");
                Logger.Info("Starting server");
                _bridge.Start();
            });
        }

        protected override void OnStop()
        {
            if (_bridge == null)
                return;

            Logger.TryLogOnException(() =>
            {
                Logger.Info("Stopping server");
                _bridge.Stop();
            });
        }
    }
}