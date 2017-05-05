using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using PagarMe.Mpos.Bridge.Providers;
using PagarMe.Mpos.Bridge.WebSocket;
using PagarMe.Mpos.Devices;
using WebSocketSharp.Server;
using NLog.Targets;
using System.IO;

namespace PagarMe.Mpos.Bridge
{
    public class MposBridge : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, Context> _contexts
            = new Dictionary<string, Context>();

        private readonly DeviceManager _deviceManager;
        private readonly Options _options;

        private WebSocketServer _server;

        private const Int32 serviceLimit = 1;

        public Options Options { get { return _options; } }

        public DeviceManager DeviceManager { get { return _deviceManager; } }

        public MposBridge(Options options)
        {
            _options = options;
            _deviceManager = new DeviceManager(logger.TryLogOnException);
        }


        public void Start()
        {
            var addresses = Dns.GetHostAddresses(Options.BindAddress);
            _server = new WebSocketServer(addresses[0], _options.BindPort);

            _server.Log.File = getLogFileName();
            _server.AddWebSocketService("/mpos", () => new MposWebSocketBehavior(this));
            _server.Start();
        }

        public void Stop()
        {
            _server.Stop();
        }

        public void Dispose()
        {
            _deviceManager.Dispose();
        }

        public Context GetContext(string name)
        {
            name = normalize(name);

            Context context;

            lock (_contexts)
            {
                if (!_contexts.TryGetValue(name, out context))
                {
                    if (_contexts.Count >= serviceLimit)
                        return null;

                    var provider = new MposProvider();

                    context = new Context(this, provider);
                    _contexts[name] = context;
                }
            }

            return context;
        }

        public void KillContext(string name)
        {
            name = normalize(name);

            lock (_contexts)
            {
                if (_contexts.ContainsKey(name))
                {
                    _contexts.Remove(name);
                }
            }
        }

        private string normalize(string name)
        {
            return name == null || name == "" ? "<default>" : name;
        }

        private static String getLogFileName()
        {
            var fileTarget = 
                logger.Factory.Configuration
                    .FindTargetByName<FileTarget>("file");

            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            var relativeFileName = fileTarget.FileName.Render(logEventInfo);
            var absoluteFileName = Path.GetFullPath(relativeFileName);

            return absoluteFileName;
        }


    }
}