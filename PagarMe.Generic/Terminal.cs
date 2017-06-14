using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PagarMe.Generic
{
    public static class Terminal
    {
        static Terminal()
        {
            var assemblyInfo = new FileInfo(Assembly.GetEntryAssembly().Location);
            AssemblyPath = assemblyInfo.Directory.FullName;
        }

        public static readonly String AssemblyPath;

        public static Result Run(String command, params String[] args)
        {
            return run(command, args, false);
        }

        public static Result RunAsAdm(String command, params String[] args)
        {
            return run(command, args, true);
        }

        private static Result run(String command, String[] args, Boolean requestAdm)
        {
            var joinedArgs = String.Join(" ", args);

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo(command, joinedArgs)
                {
                    WorkingDirectory = AssemblyPath,
                    RedirectStandardOutput = !requestAdm,
                    RedirectStandardError = !requestAdm,
                    UseShellExecute = requestAdm,
                },
                EnableRaisingEvents = true,
            };

            if (requestAdm)
            {
                proc.StartInfo.Verb = "runas";
            }

            proc.Start();
            proc.WaitForExit();

            return new Result(proc);
        }

        public class Result
        {
            internal Result(Process proc)
            {
                Code = proc.ExitCode;

                if (proc.StartInfo.RedirectStandardOutput)
                {
                    Output = proc.StandardOutput.ReadClean();
                }

                if (proc.StartInfo.RedirectStandardError)
                {
                    Error = proc.StandardError.ReadToEnd();
                }
            }

            public Boolean Succedded => Code == 0;

            public Int32 Code { get; private set; }
            public String Output { get; private set; }
            public String Error { get; private set; }
        }

        public static String ReadClean(this StreamReader reader)
        {
            var result = reader.ReadToEnd();
            result = Regex.Replace(result, @"\0", "");
            return result?.Trim();
        }
    }

}