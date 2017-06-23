using PagarMe.Bifrost.Certificates.Generation;
using PagarMe.Generic;
using System;
using System.Security.Principal;

namespace PagarMe.Bifrost.Certificates
{
    class Program
    {
        static void Main(string[] args)
        {
            changeConsoleUi();

            if (!isAdministrator())
            {
                var message = "Certificate not created: the certificate can only be set under administrator permissions";
                Log.Me.Error(message);
                throw new Exception(message);
            }

            TLSConfig.Address = args.Length == 0 ? "localhost" : args[0];
            TLSConfig.Generate();

            ServiceUser.GrantLogAccess();
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
