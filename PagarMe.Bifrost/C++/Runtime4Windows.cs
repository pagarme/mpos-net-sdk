using Microsoft.Win32;
using PagarMe.Generic;
using System;
using System.Threading.Tasks;

namespace PagarMe.Bifrost.CPP
{
    public class Runtime4Windows
    {
        public static Task InstallIfNotFound()
        {
            return Task.Run(() => 
            {
                if (ProgramEnvironment.IsWindows)
                {
                    Log.TryLogOnException(installIfNotFound);
                }
            });
        }
        
        private static void installIfNotFound()
        {
            Log.Me.Info("Verify C++ runtime for windows");

            var archCode = Environment.Is64BitProcess ? "amd64" : "x86";
            var guid = Environment.Is64BitProcess
                ? "{f1e7e313-06df-4c56-96a9-99fdfd149c51}"
                : "{c239cea1-d49e-4e16-8e87-8c055765f7ec}";
            var keyName = $@"Installer\Dependencies\,,{archCode},14.0,bundle\Dependents\{guid}";

            var value = Registry.ClassesRoot.OpenSubKey(keyName);
            var needInstall = value == null;

            if (needInstall)
            {
                Log.Me.Info("Installing C++ runtime");
                var arch = Environment.Is64BitProcess ? "64" : "86";
                var result = Terminal.Run($"vc_redist.x{arch}.exe", "/q");

                if (!result.Succedded)
                {
                    var message =
                        "Could not install Visual C++ Runtime:" + Environment.NewLine
                        + result.Output + Environment.NewLine
                        + result.Error + Environment.NewLine;

                    throw new Exception(message);
                }
            }

            Log.Me.Info("C++ runtime installed");
        }
    }
}
