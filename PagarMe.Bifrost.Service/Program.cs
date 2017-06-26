using PagarMe.Bifrost.Updates;
using PagarMe.Generic;
using System;
using System.ServiceProcess;

namespace PagarMe.Bifrost.Service
{
    internal static class Program
    {
        private static void Main(String[] args)
        {
            Log.TryLogOnException(() =>
            {
                var updater = Updater.CheckAndUpdate(MposBridge.LockContexts);

                ServiceBase.Run(new BifrostService());

                updater.Wait();
            });
        }
    }
}