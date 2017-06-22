using System;
using System.Reflection;

namespace PagarMe.Generic
{
    public class ProgramEnvironment
    {
        public static String CurrentVersion => Assembly.GetEntryAssembly().GetName().Version.ToString(3);

        private static PlatformID platform => Environment.OSVersion.Platform;
        public static Boolean IsUnix => platform == PlatformID.Unix;
        public static Boolean IsWindows => platform != PlatformID.Unix && platform != PlatformID.MacOSX;
    }
}
