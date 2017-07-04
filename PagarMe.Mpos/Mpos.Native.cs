using PagarMe.Mpos.Abecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PagarMe.Mpos
{
    public partial class Mpos
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Native
        {
            internal enum Error
            {
                ConnError = -1,
                Ok = 0,
                Error
            }

            public enum CaptureMethod
            {
                Magstripe = 0,
                EMV = 3
            }

            public enum Decision
            {
                Approved = 0,
                Refused,
                GoOnline
            }

            public enum TransactionStatus
            {
                Ok = 0,
                Error = 1,
                NonZero = 9
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct Acquirer
            {
                public int Number;
                public int CryptographyMethod;
                public int KeyIndex;

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] SessionKey;

                public Acquirer(AcquirerEntry e)
                {
                    Number = e.Number;
                    CryptographyMethod = e.CryptographyMethod;
                    KeyIndex = e.KeyIndex;

                    if (e.SessionKey != null) SessionKey = GetHexBytes(e.SessionKey, 32);
                    else SessionKey = GetHexBytes("", 32);
                }
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct RiskManagement
            {
                public int AcquirerNumber;
                public int RecordNumber;

                [MarshalAs(UnmanagedType.I1)] public bool MustRiskManagement;
                public int FloorLimit;
                public int BiasedRandomSelectionPercentage;
                public int BiasedRandomSelectionThreshold;
                public int BiasedRandomSelectionMaxPercentage;

                public RiskManagement(RiskManagementEntry e)
                {
                    AcquirerNumber = e.AcquirerNumber;
                    RecordNumber = e.RecordNumber;

                    MustRiskManagement = e.MustRiskManagement;
                    FloorLimit = e.FloorLimit;
                    BiasedRandomSelectionPercentage = e.BiasedRandomSelectionPercentage;
                    BiasedRandomSelectionThreshold = e.BiasedRandomSelectionThreshold;
                    BiasedRandomSelectionMaxPercentage = e.BiasedRandomSelectionMaxPercentage;
                }
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct Application
            {
                public int PaymentMethod;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)] public string CardBrand;

                public int AcquirerNumber;
                public int RecordNumber;

                public int EmvTagsLength;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public int[] EmvTags;

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
                }
            }

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

            public static byte[] GetBytes(string data, int length, out int newSize, char? fill = null,
                bool padLeft = true)
            {
                newSize = Encoding.UTF8.GetByteCount(data);

                if (fill.HasValue && data.Length < length)
                    data = padLeft ? data.PadLeft(length, fill.Value) : data.PadRight(length, fill.Value);

                var result = Encoding.UTF8.GetBytes(data);
                var full = new byte[length];

                Buffer.BlockCopy(result, 0, full, 0, result.Length);

                return full;
            }

            public static byte[] GetBytes(string data, int length, char? fill = null, bool padLeft = true)
            {
                int newSize;

                return GetBytes(data, length, out newSize, fill, padLeft);
            }

            public static byte[] GetHexBytes(string data, int length, out int byteLength, bool padLeft = true)
            {
                var result = GetBytes(data, length, out byteLength, '0', padLeft);

                byteLength /= 2;

                return result;
            }

            public static byte[] GetHexBytes(string data, int length, bool padLeft = true)
            {
                int newSize;

                return GetHexBytes(data, length, out newSize, padLeft);
            }

            public static byte[] GetHexBytes(byte[] data, int length, out int byteLength, bool padLeft = true)
            {
                return GetHexBytes(data.Select(x => x.ToString("X2")).Aggregate((a, b) => a + b), length, out byteLength,
                    padLeft);
            }

            public static byte[] GetHexBytes(byte[] data, int length, bool padLeft = true)
            {
                return GetHexBytes(data.Select(x => x.ToString("X2")).Aggregate((a, b) => a + b), length, padLeft);
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void MposNotificationCallbackDelegate(IntPtr mpos, string notification);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void MposOperationCompletedCallbackDelegate(IntPtr mpos);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposInitializedCallbackDelegate(IntPtr mpos, int error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposPaymentCallbackDelegateInterop(IntPtr mpos, int error, IntPtr info);
            public delegate Error MposPaymentCallbackDelegate(IntPtr mpos, int error, PaymentInfo info);
            
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposTablesLoadedCallbackDelegate(IntPtr mpos, int error, bool loaded);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposFinishTransactionCallbackDelegate(IntPtr mpos, int error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposExtractKeysCallbackDelegateInterop(IntPtr mpos, int error, IntPtr keys, int keysLength);
            public delegate Error MposExtractKeysCallbackDelegate(IntPtr mpos, int error, int[] keyList);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposGetTableVersionCallbackDelegateInterop(IntPtr mpos, int error, IntPtr version);
            public delegate Error MposGetTableVersionCallbackDelegate(IntPtr mpos, int error, String version);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error MposClosedCallbackDelegate(IntPtr mpos, int error);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate Error TmsStoreCallbackDelegateInterop(
                string version, IntPtr tables, int tableLen, IntPtr applications, int appLen, IntPtr riskManagement,
                int riskmanLen, IntPtr acquirers, int acqLen, IntPtr userData);
            public delegate Error TmsStoreCallbackDelegate(
                string version, IList<Capk> capkList, IList<Aid> aidList, IList<Application> appList,
                IList<RiskManagement> riskProfileList, IList<Acquirer> acquirerList, IntPtr userData);


            private readonly static INativeImport Dll = Environment.Is64BitProcess ? NativeBit64.Dll : NativeBit32.Dll;

            public static IntPtr Create(AbecsStream stream, MposNotificationCallbackDelegate notificationCallback, MposOperationCompletedCallbackDelegate operationCompletedCallback)
            {
                return Dll.Create(stream, notificationCallback, operationCompletedCallback);
            }

            public static Error Initialize(IntPtr mpos, MposInitializedCallbackDelegate initializedCallback)
            {
                return Dll.Initialize(mpos, initializedCallback);
            }




            public static Error ProcessPayment(IntPtr mpos, int amount, Application[] applicationList, int applicationListLength, Acquirer[] acquirers, int acquirerListLength, RiskManagement[] riskManagementList, int riskManagementListLength, int magstripePaymentMethod, MposPaymentCallbackDelegate paymentCallback)
            {
                return Dll.ProcessPayment(mpos, amount, applicationList, applicationListLength, acquirers, acquirerListLength, riskManagementList, riskManagementListLength, magstripePaymentMethod, paymentCallback);
            }



            public static Error UpdateTables(Mpos mpos, MposTablesLoadedCallbackDelegate tableCallback, Aid[] aidList, Capk[] capkList)
            {
                return Dll.UpdateTables(
                    mpos.nativeMpos, mpos.TMSStorage.GetGlobalVersion(), true, 
                    tableCallback, aidList, capkList
                );
            }

            public static Error FinishTransaction(IntPtr mpos, TransactionStatus status, int arc, string emv, int emvLen, MposFinishTransactionCallbackDelegate callback)
            {
                return Dll.FinishTransaction(mpos, status, arc, emv, emvLen, callback);
            }

            public static Error ExtractKeys(IntPtr mpos, MposExtractKeysCallbackDelegate callback)
            {
                return Dll.ExtractKeys(mpos, callback);
            }

            public static Error GetTableVersion(IntPtr mpos, MposGetTableVersionCallbackDelegate callback)
            {
                return Dll.GetTableVersion(mpos, callback);
            }

            public static Error Display(IntPtr mpos, string text)
            {
                return Dll.Display(mpos, text);
            }

            public static Error Close(IntPtr mpos, string text, MposClosedCallbackDelegate callback)
            {
                return Dll.Close(mpos, text, callback);
            }

            public static Error Free(IntPtr mpos)
            {
                return Dll.Free(mpos);
            }

            public static Error Cancel(IntPtr mpos)
            {
                return Dll.Cancel(mpos);
            }

            public static Error TmsGetTables(string payload, int length, TmsStoreCallbackDelegate callback, IntPtr userData)
            {
                return Dll.TmsGetTables(payload, length, callback, userData);
            }

            
        }
    }
}