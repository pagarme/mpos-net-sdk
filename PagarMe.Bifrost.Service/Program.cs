using PagarMe.Bifrost.Updates;
using System;
using System.ServiceProcess;

namespace PagarMe.Bifrost.Service
{
    internal static class Program
    {
        private static void Main(String[] args)
        {
            var updater = WindowsUpdater.CheckAndUpdate();

            ServiceBase.Run(new BifrostService());

            updater.Wait();
        }
    }
}