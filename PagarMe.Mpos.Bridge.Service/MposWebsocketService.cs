using System.ServiceProcess;
using CommandLine;
using NLog;
using System;

namespace PagarMe.Mpos.Bridge.Service
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
            try
            {
                var options = new Options();
                var isValid = Parser.Default.ParseArgumentsStrict(args, options);
                options.EnsureDefaults();

                _bridge = new MposBridge(options);

                Logger.Info("mPOS Websocket Bridge");
                Logger.Info("Starting server");
                _bridge.Start();
            }
            catch(Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        protected override void OnStop()
        {
            if (_bridge == null)
                return;
            try
            {
                Logger.Info("Stopping server");
                _bridge.Stop();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }
    }
}