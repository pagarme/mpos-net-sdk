using System;
using System.Reflection;

namespace PagarMe.Generic
{
    public class ProgramEnvironment
    {
        public static Version CurrentVersion => Assembly.GetEntryAssembly().GetName().Version;

        private static PlatformID platform => Environment.OSVersion.Platform;
        public static Boolean IsUnix => platform == PlatformID.Unix;
        public static Boolean IsWindows => platform != PlatformID.Unix && platform != PlatformID.MacOSX;
    }
}
