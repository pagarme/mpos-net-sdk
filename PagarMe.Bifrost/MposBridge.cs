using System;
using System.Collections.Generic;
using System.Net;
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
        private static readonly Dictionary<string, Context> contexts
            = new Dictionary<string, Context>();

        private static Boolean contextsLocked = false;

        private readonly DeviceManager deviceManager;
        private readonly Options options;

        private WebSocketServer server;

        public Options Options { get { return options; } }

        public DeviceManager DeviceManager { get { return deviceManager; } }

        public MposBridge(Options options)
        {
            this.options = options;
            deviceManager = new DeviceManager(Log.TryLogOnException);
        }


        public void Start(Boolean ssl = true)
        {
            var addresses = Dns.GetHostAddresses(Options.BindAddress);
            server = new WebSocketServer(addresses[0], options.BindPort, ssl);

            TLSConfig.Address = Options.BindAddress;

            if (ssl)
            {
                server.SslConfiguration.ServerCertificate = TLSConfig.Get();

                server.SslConfiguration.CheckCertificateRevocation = false;
                server.SslConfiguration.ClientCertificateRequired = false;
                server.SslConfiguration.ClientCertificateValidationCallback = TLSConfig.ClientValidate;

                server.KeepClean = false;
            }

            server.Log.File = Log.GetLogFilePath();

            server.AddWebSocketService("/mpos", () => new BifrostBehavior(this));
            server.Start();
        }

        public void Stop()
        {
            server.Stop();
        }

        public void Dispose()
        {
            deviceManager.Dispose();
        }

        public String GetDeviceContextName(String deviceId)
        {
            lock (contexts)
            {
                return contexts
                    .Where(c => c.Value != null && c.Value.DeviceId == deviceId)
                    .Select(c => c.Key)
                    .SingleOrDefault();
            }
        }

        public Context GetContext(string name)
        {
            if (contextsLocked) return null;

            name = normalize(name);

            Context context;

            lock (contexts)
            {
                if (!contexts.TryGetValue(name, out context))
                {
                    var provider = new MposProvider();

                    context = new Context(this, provider);
                    contexts[name] = context;
                }
            }

            return context;
        }

        public async Task KillContext(string name)
        {
            name = normalize(name);
            Context context = null;

            lock (contexts)
            {
                if (contexts.ContainsKey(name))
                {
                    context = contexts[name];
                    contexts.Remove(name);
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
            lock (contexts)
            {
                if (contexts.Any(c => c.Value.IsInUse()))
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