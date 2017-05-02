using PagarMe.Mpos;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static PagarMe.Mpos.Mpos.Native;

namespace PagarMe.Mock.LibC
{
    partial class DllMock : INativeImport
    {
        private struct Pointer
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] objPtr;
        }

        private Pointer pointer = new Pointer()
        {
            objPtr = Encoding.UTF8.GetBytes("MockPointer")
        };


        private IntPtr mpos;

        public IntPtr ReceivedMpos { get; private set; }

        private String notification = "Automated test running";

        private Boolean loaded = true;

        private Int32 error = 0;
        internal Error resultInit = Error.Ok;
        internal Error resultTableUpdate = Error.Ok;
        private Error result = Error.Ok;

        private String version = "3";
        private Int32[] keyList = new[] { 1, 2, 3 };

        private static Capk capkMock = new Capk(new CapkEntry
        {
            AcquirerNumber = 1,
            Checksum = "Checksum",
            Exponent = "000000",
            Modulus = "Modulus",
            PublicKeyId = 1,
            RecordIndex = 1,
            Rid = "Rid"
        });
        private IList<Capk> capkList = new List<Capk> { capkMock };

        private static Aid aidMock = new Aid(new AidEntry
        {
            AcquirerNumber = 1,
            AdditionalTerminalCapabilities = "0000000000",
            Aid = "Aid",
            ApplicationName = "ApplicationName",
            ApplicationType = 1,
            AppVersion1 = "1111",
            AppVersion2 = "2222",
            AppVersion3 = "3333",
            CountryCode = 55,
            CtlsApplicationVersion = "0000",
            CtlsCvmLimit = 1,
            CtlsFloorLimit = 1,
            CtlsMode = 1, 
            CtlsTransactionLimit = 1,
            CtlsZeroAm = false,
            Currency = 1,
            CurrencyExponent = 1,
            Ddol = "Ddol",
            DefaultTac = "DefaultTac",
            DenialTac = "DenialTac",
            FloorLimit = 1,
            Mcc = 1,
            MerchantId = "MerchantId",
            OnlineTac = "OnlineTac",
            RecordIndex = 2,
            Tcc = "Tcc",
            Tdol = "Tdol",
            TerminalCapabilities = "000000",
            TerminalId = "00000000",
            TerminalType = 1,
        });
        private IList<Aid> aidList = new List<Aid> { aidMock };

        private static Application appMock = new Application(new ApplicationEntry
        {
            AcquirerNumber = 1,
            CardBrand = "CardBrand",
            EmvTags = "2,3,5,7",
            PaymentMethod = 1,
            RecordNumber = 3,
        });
        private IList<Application> appList = new List<Application> { appMock };

        private static RiskManagement riskMock = new RiskManagement(new RiskManagementEntry
        {
            AcquirerNumber = 1,
            BiasedRandomSelectionMaxPercentage = 1,
            BiasedRandomSelectionPercentage = 1,
            BiasedRandomSelectionThreshold = 1,
            FloorLimit = 1,
            MustRiskManagement = false,
            RecordNumber = 4,
        });
        private IList<RiskManagement> riskProfileList = new List<RiskManagement> { riskMock };

        private static Acquirer acquirerMock = new Acquirer(new AcquirerEntry
        {
            CryptographyMethod = 1,
            KeyIndex = 1,
            Number = 5,
            SessionKey = "SessionKey",
        });
        private IList<Acquirer> acquirerList = new List<Acquirer> { acquirerMock };


        private static Str2Bytes emvData = new Str2Bytes("EmvData");
        private static Str2Bytes expirationDate = new Str2Bytes("2017-03-28 20:19");
        private static Str2Bytes holderName = new Str2Bytes("HolderName");
        private static Str2Bytes pan = new Str2Bytes("Pan");
        private static Str2Bytes pin = new Str2Bytes("Pin");
        private static Str2Bytes pinKek = new Str2Bytes("PinKek");
        private static Str2Bytes track1 = new Str2Bytes("Track1");
        private static Str2Bytes track2 = new Str2Bytes("Track2");
        private static Str2Bytes track3 = new Str2Bytes("Track3");
        private PaymentInfo info = new PaymentInfo {
            AcquirerIndex = 1,
            Amount = 100,
            ApplicationType = 1,
            CaptureMethod = Mpos.Mpos.Native.CaptureMethod.EMV,
            Decision = Decision.Approved,
            EmvData = emvData.Bytes,
            EmvDataLength = emvData.Length,
            ExpirationDate = expirationDate.Bytes,
            HolderName = holderName.Bytes,
            HolderNameLength = holderName.Length,
            IsOnlinePin = 1,
            Pan = pan.Bytes,
            PanLength = pan.Length,
            PanSequenceNumber = 1,
            Pin = pin.Bytes,
            PinKek = pinKek.Bytes,
            PinRequired = 1,
            RecordNumber = new Random().Next(),
            Track1 = track1.Bytes,
            Track1Length = track1.Length,
            Track2 = track2.Bytes,
            Track2Length = track2.Length,
            Track3 = track3.Bytes,
            Track3Length = track3.Length,
        };

        
        class Str2Bytes
        {
            public Str2Bytes(String text)
            {
                Bytes = bytes(text);
                Length = new IntPtr(text.Length);
            }

            public readonly Byte[] Bytes;
            public readonly IntPtr Length;

            private static byte[] bytes(String word)
            {
                return Encoding.UTF8.GetBytes(word);
            }
        }





    }
}
