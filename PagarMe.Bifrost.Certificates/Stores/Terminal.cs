using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PagarMe.Bifrost.Certificates.Stores
{
    static class Terminal
    {
        static Terminal()
        {
            var assemblyInfo = new FileInfo(Assembly.GetEntryAssembly().Location);
            AssemblyPath = assemblyInfo.Directory.FullName;
        }

        public static readonly String AssemblyPath;

        public static Int32 Run(String command, params String[] args)
        {
            var joinedArgs = String.Join(" ", args);

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo(command, joinedArgs)
                {
                    WorkingDirectory = AssemblyPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                },
                EnableRaisingEvents = true,
            };

            proc.Start();
            proc.WaitForExit();

            return proc.ExitCode;
        }
    }

}