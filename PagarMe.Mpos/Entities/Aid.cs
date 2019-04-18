using System;
using System.Runtime.InteropServices;
using PagarMe.Mpos.Tms;
using static PagarMe.Mpos.Natives.Native;

namespace PagarMe.Mpos.Entities
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Aid
    {
        [MarshalAs(UnmanagedType.I1)] public bool IsAid;
        public int AcquirerNumber;
        public int RecordIndex;

        public int AidLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] AidNumber;
        public int ApplicationType;
        public int ApplicationNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] ApplicationName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] AppVersion1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] AppVersion2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] AppVersion3;
        public int CountryCode;
        public int Currency;
        public int CurrencyExponent;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)] public byte[] MerchantId;
        public int Mcc;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] TerminalId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public byte[] TerminalCapabilities;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] AdditionalTerminalCapabilities;
        public int TerminalType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] DefaultTac;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] DenialTac;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] OnlineTac;
        public int FloorLimit;
        [MarshalAs(UnmanagedType.I1)] public byte Tcc;

        [MarshalAs(UnmanagedType.I1)] public bool CtlsZeroAm;
        public int CtlsMode;
        public int CtlsTransactionLimit;
        public int CtlsFloorLimit;
        public int CtlsCvmLimit;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] CtlsApplicationVersion;

        public int TdolLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] Tdol;
        public int DdolLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] Ddol;

        public Aid(AidEntry e)
        {
            IsAid = true;
            AcquirerNumber = e.AcquirerNumber;
            RecordIndex = e.RecordIndex;

            AidNumber = GetBytes(e.Aid, 32, out AidLength);
            AidLength /= 2;
            ApplicationType = e.ApplicationType;
            ApplicationName = GetBytes(e.ApplicationName, 16, out ApplicationNameLength);
            AppVersion1 = GetBytes(e.AppVersion1, 4);
            AppVersion2 = GetBytes(e.AppVersion2, 4);
            AppVersion3 = GetBytes(e.AppVersion3, 4);
            CountryCode = e.CountryCode;
            Currency = e.Currency;
            CurrencyExponent = e.CurrencyExponent;
            MerchantId = GetHexBytes("", 15);
            Mcc = e.Mcc;
            TerminalId = GetBytes(e.TerminalId, 8);

            TerminalCapabilities = GetBytes(e.TerminalCapabilities, 6);
            AdditionalTerminalCapabilities = GetBytes(e.AdditionalTerminalCapabilities, 10);
            TerminalType = e.TerminalType;
            DefaultTac = GetBytes(e.DefaultTac, 10);
            DenialTac = GetBytes(e.DenialTac, 10);
            OnlineTac = GetBytes(e.OnlineTac, 10);
            FloorLimit = e.FloorLimit;
            Tcc = Convert.ToByte(e.Tcc[0]);

            CtlsZeroAm = e.CtlsZeroAm;
            CtlsMode = e.CtlsMode;
            CtlsTransactionLimit = e.CtlsTransactionLimit;
            CtlsFloorLimit = e.CtlsFloorLimit;
            CtlsCvmLimit = e.CtlsCvmLimit;
            CtlsApplicationVersion = GetBytes(e.CtlsApplicationVersion, 4);

            Tdol = GetBytes(e.Tdol, 40, out TdolLength);
            Ddol = GetBytes(e.Ddol, 40, out DdolLength);
        }
    }
}
