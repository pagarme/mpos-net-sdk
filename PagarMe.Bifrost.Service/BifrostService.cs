using System.ServiceProcess;
using PagarMe.Generic;

namespace PagarMe.Bifrost.Service
{
    public partial class BifrostService : ServiceBase
    {
        private readonly Options options;
        private MposBridge _bridge;

        public BifrostService(Options options)
        {
            this.options = options;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log.TryLogOnException(() =>
            {
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