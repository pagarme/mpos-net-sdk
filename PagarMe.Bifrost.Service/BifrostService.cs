using System.ServiceProcess;
using CommandLine;
using PagarMe.Generic;

namespace PagarMe.Bifrost.Service
{
    public partial class BifrostService : ServiceBase
    {
        private MposBridge _bridge;

        public BifrostService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log.TryLogOnException(() =>
            {
                var options = new Options();
                var isValid = Parser.Default.ParseArgumentsStrict(args, options);
                options.EnsureDefaults();

                _bridge = new MposBridge(options);

                Log.Me.Info("Bifrost Service Bridge");
                Log.Me.Info("Starting server");
                _bridge.Start();
            });
        }

        protected override void OnStop()
        {
            if (_bridge == null)
                return;

            Log.TryLogOnException(() =>
            {
                Log.Me.Info("Stopping server");
                _bridge.Stop();
            });
        }
    }
}