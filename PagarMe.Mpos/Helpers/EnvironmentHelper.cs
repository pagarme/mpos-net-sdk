using System;
using System.IO;
using System.Reflection;

namespace PagarMe.Mpos.Helpers
{
    class EnvironmentHelper
    {
        static String extension = Environment.OSVersion.Platform == PlatformID.Unix ? "so" : "dll";
        static Int32 architeture = Environment.Is64BitProcess ? 64 : 32;

        public static void CopyAllDll()
        {
            copyDll("mpos");
            copyDll("tms");
            copyDll("sqlite3_", "sqlite3");
        }

        private static void copyDll(string name, String newName = null)
        {
            var chosenDllName = $"{name}{architeture}.{extension}";
            newName = newName ?? name;
            var loadDllName = $"{newName}.{extension}";

            var dllPath = Assembly.GetExecutingAssembly().Location;
            var dllDirectory = Path.GetDirectoryName(dllPath);

            var chosenDllPath = Path.Combine(dllDirectory, chosenDllName);
            var loadDllPath = Path.Combine(dllDirectory, loadDllName);

            if (!File.Exists(chosenDllPath) || File.Exists(loadDllPath))
                return;

            File.Copy(chosenDllPath, loadDllPath);
        }
    }
}
