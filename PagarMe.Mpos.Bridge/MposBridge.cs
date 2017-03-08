using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using PagarMe.Mpos.Bridge.Providers;
using PagarMe.Mpos.Bridge.WebSocket;
using PagarMe.Mpos.Devices;
using WebSocketSharp.Server;

namespace PagarMe.Mpos.Bridge
{
    public class MposBridge : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, Context> _contexts;
        private readonly DeviceManager _deviceManager;
        private readonly Options _options;

        public Options Options { get { return _options; } }

        public DeviceManager DeviceManager { get { return _deviceManager; } }

        public MposBridge(Options options)
        {
            _options = options;
            _contexts = new Dictionary<string, Context>();
            _deviceManager = new DeviceManager();
        }


        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
            _deviceManager.Dispose();
        }

        public Context GetContext(string name)
        {
            Context context;

            if (name == null || name == "")
                name = "<default>";

            lock (_contexts)
            {
                if (!_contexts.TryGetValue(name, out context))
                {
                    var provider = new MposProvider();

                    context = new Context(this, provider);
                    _contexts[name] = context;
                }
            }

            return context;
        }
    }
}