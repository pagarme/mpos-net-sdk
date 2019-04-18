using System.Runtime.InteropServices;
using PagarMe.Mpos.Tms;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos.Entities
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Capk
    {
        [MarshalAs(UnmanagedType.I1)] public bool IsAid;
        public int AcquirerNumber;
        public int RecordIndex;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] Rid;
        public int CapkIndex;
        public int ExponentLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public byte[] Exponent;
        public int ModulusLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 496)] public byte[] Modulus;

        [MarshalAs(UnmanagedType.I1)] public bool HasChecksum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] Checksum;

        public Capk(CapkEntry e)
        {
            IsAid = false;
            AcquirerNumber = e.AcquirerNumber;
            RecordIndex = e.RecordIndex;

            Rid = GetBytes(e.Rid, 10);
            CapkIndex = e.PublicKeyId;
            Exponent = GetBytes(e.Exponent, 6, out ExponentLength);
            ExponentLength /= 2;
            Modulus = GetBytes(e.Modulus, 496, out ModulusLength);
            ModulusLength /= 2;
            HasChecksum = e.Checksum != null;

            if (HasChecksum)
                Checksum = GetBytes(e.Checksum, 40);
            else
                Checksum = GetHexBytes("", 40);
        }
    }
}
