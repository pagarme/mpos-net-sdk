using System.ServiceProcess;
using CommandLine;
using NLog;
using PagarMe.Generic;

namespace PagarMe.Bifrost.Service
{
    public partial class BifrostService : ServiceBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private MposBridge _bridge;

        public BifrostService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            logger.TryLogOnException(() =>
            {
                var options = new Options();
                var isValid = Parser.Default.ParseArgumentsStrict(args, options);
                options.EnsureDefaults();

                _bridge = new MposBridge(options);

                logger.Info("Bifrost Service Bridge");
                logger.Info("Starting server");
                _bridge.Start();
            });
        }

        protected override void OnStop()
        {
            if (_bridge == null)
                return;

            logger.TryLogOnException(() =>
            {
                logger.Info("Stopping server");
                _bridge.Stop();
            });
        }
    }
}