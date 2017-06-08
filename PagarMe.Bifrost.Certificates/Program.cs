using NLog;
using PagarMe.Bifrost.Certificates.Generation;
using System;
using System.Security.Principal;

namespace PagarMe.Bifrost.Certificates
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            if (!isAdministrator())
            {
                var message = "Certificate not created: the certificate can only be set under administrator permissions";
                logger.Error(message);
                throw new Exception(message);
            }

            TLSConfig.Address = args.Length == 0 ? "localhost" : args[0];
            TLSConfig.Generate();

            TLSConfig.GrantLogAccess();
        }

        private static bool isAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

    }
}
