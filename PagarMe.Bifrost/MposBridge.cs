using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using PagarMe.Bifrost.Providers;
using PagarMe.Bifrost.WebSocket;
using PagarMe.Mpos.Devices;
using WebSocketSharp.Server;
using PagarMe.Generic;
using PagarMe.Bifrost.Certificates.Generation;
using System.Linq;
using System.Threading.Tasks;

namespace PagarMe.Bifrost
{
    public class MposBridge : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, Context> _contexts
            = new Dictionary<string, Context>();

        private static Boolean contextsLocked = false;

        private readonly DeviceManager _deviceManager;
        private readonly Options _options;

        private WebSocketServer _server;

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
            _server = new WebSocketServer(addresses[0], _options.BindPort, true);

            TLSConfig.Address = Options.BindAddress;
            _server.SslConfiguration.ServerCertificate = TLSConfig.Get();

            _server.SslConfiguration.CheckCertificateRevocation = false;
            _server.SslConfiguration.ClientCertificateRequired = false;
            _server.SslConfiguration.ClientCertificateValidationCallback = TLSConfig.ClientValidate;

            _server.KeepClean = false;
            _server.Log.File = logger.GetLogFilePath();

            _server.AddWebSocketService("/mpos", () => new BifrostBehavior(this));
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

        public String GetDeviceContextName(String deviceId)
        {
            lock (_contexts)
            {
                return _contexts
                    .Where(c => c.Value != null && c.Value.DeviceId == deviceId)
                    .Select(c => c.Key)
                    .SingleOrDefault();
            }
        }

        public Context GetContext(string name)
        {
            name = normalize(name);

            Context context;

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

        public async Task KillContext(string name)
        {
            name = normalize(name);
            Context context = null;

            lock (_contexts)
            {
                if (_contexts.ContainsKey(name))
                {
                    context = _contexts[name];
                    _contexts.Remove(name);
                }
            }

            if (context != null)
            {
                await context.Close();
                context.Dispose();
            }
        }

        public static Boolean LockContexts()
        {
            lock (_contexts)
            {
                if (_contexts.Any(c => c.Value.IsInUse()))
                {
                    return false;
                }

                contextsLocked = true;

                return true;
            }
        }

        private string normalize(string name)
        {
            return name == null || name == "" ? "<default>" : name;
        }


    }
}