using System.Linq;
using System.Runtime.InteropServices;
using PagarMe.Mpos.Tms;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos.Entities
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Application
    {
        public int PaymentMethod;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)] public string CardBrand;

        public int AcquirerNumber;
        public int RecordNumber;

        public int EmvTagsLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public int[] EmvTags;

        public bool CtlsZeroAm;
        public int CtlsMode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] CtlsTransactionLimit;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] CtlsFloorLimit;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] CtlsCvmLimit;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] CtlsApplicationVersion;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] CtlsDefaultTac;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] CtlsDenialTac;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] CtlsOnlineTac;

        public Application(ApplicationEntry e)
        {
            PaymentMethod = e.PaymentMethod;
            CardBrand = e.CardBrand;

            AcquirerNumber = e.AcquirerNumber;
            RecordNumber = e.RecordNumber;

            EmvTags = new int[256];
            var tags = e.EmvTags.Split(',').Select(int.Parse).ToArray();
            for (var i = 0; i < tags.Length; i++)
                EmvTags[i] = tags[i];
            EmvTagsLength = tags.Length;
            
            CtlsZeroAm = e.CtlsZeroAm;
            CtlsMode = e.CtlsMode;

            CtlsTransactionLimit = GetBytes(e.CtlsTransactionLimit, 8);
            CtlsFloorLimit = GetBytes(e.CtlsFloorLimit, 8);
            CtlsCvmLimit = GetBytes(e.CtlsCvmLimit, 8);
            CtlsApplicationVersion = GetBytes(e.CtlsApplicationVersion, 4);

            CtlsDefaultTac = GetBytes(e.CtlsDefaultTac, 10);
            CtlsDenialTac = GetBytes(e.CtlsDenialTac, 10);
            CtlsOnlineTac = GetBytes(e.CtlsOnlineTac, 10);
        }
    }
}
