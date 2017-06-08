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
            changeConsoleUi();

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

        private static void changeConsoleUi()
        {
            Console.SetWindowSize(28, 3);
            Console.BufferWidth = 28;
            Console.BufferHeight = 3;
            Console.Title = "SSL";
            Console.CursorVisible = false;
            Console.WriteLine("   ------------------------");
            Console.WriteLine("     Enabling Bifrost SSL  ");
            Console.Write    ("   ------------------------");
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
