using PagarMe.Bifrost.Updates;
using System;
using System.ServiceProcess;

namespace PagarMe.Bifrost.Service
{
    internal static class Program
    {
        private static void Main(String[] args)
        {
            var updater = Updater.CheckAndUpdate(MposBridge.LockContexts);

            ServiceBase.Run(new BifrostService());

            updater.Wait();
        }
    }
}