using System.ServiceProcess;
using PagarMe.Generic;

namespace PagarMe.Bifrost.Service
{
    public partial class BifrostService : ServiceBase
    {
        private readonly Options options;
        private MposBridge bridge;

        public BifrostService(Options options)
        {
            this.options = options;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log.TryLogOnException(() =>
            {
                bridge = new MposBridge(options);

                Log.Me.Info("Bifrost Service Bridge");
                Log.Me.Info("Starting server");
                bridge.Start();
            });
        }

        protected override void OnStop()
        {
            if (bridge == null)
                return;

            Log.TryLogOnException(() =>
            {
                Log.Me.Info("Stopping server");
                bridge.Stop();
            });
        }
    }
}