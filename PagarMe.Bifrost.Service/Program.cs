using System.ServiceProcess;

namespace PagarMe.Bifrost.Service
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            ServiceBase.Run(new BifrostService());
        }
    }
}