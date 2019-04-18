using System;
using System.Runtime.InteropServices;

namespace PagarMe.Mpos.Entities
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct PaymentInfo
    {
        [MarshalAs(UnmanagedType.I4)] public CaptureMethod CaptureMethod;

        [MarshalAs(UnmanagedType.I4)] public Decision Decision;

        public int Amount;
        public int AcquirerIndex;
        public int RecordNumber;
        public int ApplicationType;
        public int PanSequenceNumber;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public byte[] ExpirationDate;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)] public byte[] HolderName;
        public IntPtr HolderNameLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)] public byte[] Pan;
        public IntPtr PanLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 76)] public byte[] Track1;
        public IntPtr Track1Length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] Track2;
        public IntPtr Track2Length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 104)] public byte[] Track3;
        public IntPtr Track3Length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)] public byte[] EmvData;
        public IntPtr EmvDataLength;

        public int PinRequired;
        public int IsOnlinePin;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] Pin;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] PinKek;
    }
}
