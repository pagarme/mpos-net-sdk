using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PagarMe.Mpos.Abecs;

namespace PagarMe.Mpos
{
    public partial class Mpos : IDisposable
    {
        private readonly IntPtr _nativeMpos;

        private AbecsStream _stream;

        private readonly Native.MposNotificationCallbackDelegate NotificationPin;
        private readonly Native.MposOperationCompletedCallbackDelegate OperationPin;

        public Mpos(Stream stream, string encryptionKey, string storagePath)
            : this(new AbecsStream(stream), encryptionKey, storagePath) { }

        private unsafe Mpos(AbecsStream stream, string encryptionKey, string storagePath)
        {
            NotificationPin = HandleNotificationCallback;
            OperationPin = HandleOperationCompletedCallback;

            _stream = stream;
            EncryptionKey = encryptionKey;
            StoragePath = storagePath;
            _nativeMpos = Native.Create((IntPtr) stream.NativeStream, NotificationPin, OperationPin);
            TMSStorage = new TMSStorage(storagePath, encryptionKey);
        }

        public Stream BaseStream
        {
            get { return _stream.BaseStream; }
        }

        public string EncryptionKey { get; }
        public string StoragePath { get; }
        public TMSStorage TMSStorage { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static string GetString(byte[] data, IntPtr len)
        {
            return GetString(data, len.ToInt32());
        }

        private static string GetString(byte[] data, int len = -1)
        {
            if (len == -1)
                len = data.Length;

            return Encoding.ASCII.GetString(data, 0, len);
        }

        private static IntPtr GetMarshalBytes<T>(T str)
        {
            var size = Marshal.SizeOf(typeof(T));

            var ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, false);

            return ptr;
        }

        public event EventHandler<int> Errored;
        public event EventHandler Initialized;
        public event EventHandler Closed;
        public event EventHandler<PaymentResult> PaymentProcessed;
        public event EventHandler<bool> TableUpdated;
        public event EventHandler FinishedTransaction;
        public event EventHandler<string> NotificationReceived;
        public event EventHandler OperationCompleted;

        ~Mpos()
        {
            Dispose(false);
        }

        public Task Initialize()
        {
            var pin = default(GCHandle);
            var source = new TaskCompletionSource<bool>();

            Native.MposInitializedCallbackDelegate callback = (mpos, err) =>
            {
                pin.Free();

                try
                {
                    OnInitialized(err);
                    source.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    source.TrySetException(ex);
                }

                return Native.Error.Ok;
            };

            pin = GCHandle.Alloc(callback);

            var error = Native.Initialize(_nativeMpos, IntPtr.Zero, callback);

            if (error != Native.Error.Ok)
                throw new MposException(error);

            return source.Task;
        }

        public Task SynchronizeTables(bool forceUpdate)
        {
            var source = new TaskCompletionSource<bool>();
            var keysPin = default(GCHandle);

            Native.MposExtractKeysCallbackDelegate keysCallback = (mpos, err, keys, keyLength) =>
            {
                var pin = default(GCHandle);

                Native.TmsStoreCallbackDelegate tmsCallback =
                    (version, tables, tableLen, applications, appLen, riskProfiles, riskLen, acquirers, acqLen, userData)
                        =>
                    {
                        pin.Free();

                        var tablePin = default(GCHandle);

                        Native.MposTablesLoadedCallbackDelegate callback = (mpos2, tableError, loaded) =>
                        {
                            tablePin.Free();

                            OnTableUpdated(loaded, tableError);
                            source.SetResult(true);

                            return Native.Error.Ok;
                        };
                        tablePin = GCHandle.Alloc(callback);

                        TMSStorage.PurgeIndex();
                        TMSStorage.StoreGlobalVersion(version);

                        for (var i = 0; i < tableLen; i++)
                        {
                            var pointer = IntPtr.Add(tables, i * Marshal.SizeOf(typeof(IntPtr)));
                            var deref = (IntPtr) Marshal.PtrToStructure(pointer, typeof(IntPtr));

                            // We assume everything is the smaller member
                            var capk = (Native.Capk) Marshal.PtrToStructure(deref, typeof(Native.Capk));
                            var isAid = capk.IsAid;

                            if (isAid)
                            {
                                var aid = (Native.Aid) Marshal.PtrToStructure(deref, typeof(Native.Aid));
                                TMSStorage.StoreAidRow(aid.AcquirerNumber, aid.RecordIndex, aid.AidLength, aid.AidNumber,
                                    aid.ApplicationType, aid.ApplicationNameLength, aid.ApplicationName, aid.AppVersion1,
                                    aid.AppVersion2, aid.AppVersion3, aid.CountryCode, aid.Currency,
                                    aid.CurrencyExponent, aid.MerchantId, aid.Mcc, aid.TerminalId,
                                    aid.TerminalCapabilities, aid.AdditionalTerminalCapabilities, aid.TerminalType,
                                    aid.DefaultTac, aid.DenialTac, aid.OnlineTac, aid.FloorLimit, aid.Tcc,
                                    aid.CtlsZeroAm, aid.CtlsMode, aid.CtlsTransactionLimit, aid.CtlsFloorLimit,
                                    aid.CtlsCvmLimit, aid.CtlsApplicationVersion, aid.TdolLength, aid.Tdol,
                                    aid.DdolLength, aid.Ddol);
                            }
                            else
                            {
                                TMSStorage.StoreCapkRow(capk.AcquirerNumber, capk.RecordIndex, capk.Rid, capk.CapkIndex,
                                    capk.ExponentLength, capk.Exponent, capk.ModulusLength, capk.Modulus,
                                    capk.HasChecksum, capk.Checksum);
                            }
                        }

                        for (var i = 0; i < appLen; i++)
                        {
                            var pointer = IntPtr.Add(applications, i * Marshal.SizeOf(typeof(IntPtr)));
                            var deref = (IntPtr) Marshal.PtrToStructure(pointer, typeof(IntPtr));

                            var app = (Native.Application) Marshal.PtrToStructure(deref, typeof(Native.Application));

                            TMSStorage.StoreApplicationRow(app.PaymentMethod, app.CardBrand, app.AcquirerNumber,
                                app.RecordNumber, app.EmvTagsLength, app.EmvTags);
                        }

                        for (var i = 0; i < appLen; i++)
                        {
                            var pointer = IntPtr.Add(riskProfiles, i * Marshal.SizeOf(typeof(IntPtr)));
                            var deref = (IntPtr) Marshal.PtrToStructure(pointer, typeof(IntPtr));

                            var profile =
                                (Native.RiskManagement) Marshal.PtrToStructure(deref, typeof(Native.RiskManagement));

                            TMSStorage.StoreRiskManagementRow(profile.AcquirerNumber, profile.RecordNumber,
                                profile.MustRiskManagement, profile.FloorLimit, profile.BiasedRandomSelectionPercentage,
                                profile.BiasedRandomSelectionThreshold, profile.BiasedRandomSelectionMaxPercentage);
                        }

                        for (var i = 0; i < acqLen; i++)
                        {
                            var pointer = IntPtr.Add(acquirers, i * Marshal.SizeOf(typeof(IntPtr)));
                            var deref = (IntPtr) Marshal.PtrToStructure(pointer, typeof(IntPtr));

                            var acquirer = (Native.Acquirer) Marshal.PtrToStructure(deref, typeof(Native.Acquirer));

                            TMSStorage.StoreAcquirerRow(acquirer.Number, acquirer.CryptographyMethod, acquirer.KeyIndex,
                                acquirer.SessionKey);
                        }

                        var updateError = Native.UpdateTables(_nativeMpos, tables, tableLen, version, forceUpdate,
                            callback);
                        if (updateError != Native.Error.Ok) throw new MposException(updateError);

                        return updateError;
                    };
                pin = GCHandle.Alloc(tmsCallback);

                var cleanKeys = new int[keyLength];
                for (var i = 0; i < keyLength; i++)
                {
                    var pointer = IntPtr.Add(keys, i * Marshal.SizeOf(typeof(int)));
                    cleanKeys[i] = (int) Marshal.PtrToStructure(pointer, typeof(int));
                }

                ApiHelper.GetTerminalTables(EncryptionKey, !forceUpdate ? TMSStorage.GetGlobalVersion() : "", cleanKeys)
                    .ContinueWith(t =>
                    {
                        if (t.Status == TaskStatus.Faulted)
                        {
                            source.SetException(t.Exception);
                            return;
                        }

                        if (t.Result.Length > 0)
                        {
                            var error = Native.TmsGetTables(t.Result, t.Result.Length, tmsCallback, IntPtr.Zero);
                            if (error != Native.Error.Ok) throw new MposException(error);
                        }

                        else
                        {
                            // We don't need to do anything; complete operation.	
                            OnTableUpdated(false, 0);
                            source.SetResult(true);
                        }
                    });

                return Native.Error.Ok;
            };
            keysPin = GCHandle.Alloc(keysCallback);

            var keysError = Native.ExtractKeys(_nativeMpos, keysCallback);
            if (keysError != Native.Error.Ok) throw new MposException(keysError);

            return source.Task;
        }

        public Task<PaymentResult> ProcessPayment(int amount, IEnumerable<EmvApplication> applications = null,
            PaymentMethod magstripePaymentMethod = PaymentMethod.Credit)
        {
            var source = new TaskCompletionSource<PaymentResult>();

            var tablePin = default(GCHandle);
            Native.MposTablesLoadedCallbackDelegate tableCallback = (mpos2, tableError, loaded) =>
            {
                tablePin.Free();

                var pin = default(GCHandle);
                Native.MposPaymentCallbackDelegate callback = (mpos, err, infoPointer) =>
                {
                    pin.Free();

                    if (err != 0)
                    {
                        OnPaymentProcessed(null, err);
                        return Native.Error.Ok;
                    }
                    var info = (Native.PaymentInfo) Marshal.PtrToStructure(infoPointer, typeof(Native.PaymentInfo));

                    HandlePaymentCallback(err, info).ContinueWith(t =>
                    {
                        if (t.Status == TaskStatus.Faulted) source.SetException(t.Exception);
                        else source.SetResult(t.Result);

                        OnPaymentProcessed(t.Result, err);
                    });

                    return Native.Error.Ok;
                };
                pin = GCHandle.Alloc(callback);

                var acquirers = new List<Native.Acquirer>();
                var riskProfiles = new List<Native.RiskManagement>();

                var rawApplications = new List<Native.Application>();
                if (applications != null)
                    foreach (var application in applications)
                    {
                        var entry = TMSStorage.SelectApplication(application.Brand, (int) application.PaymentMethod);
                        if (entry != null) rawApplications.Add(new Native.Application(entry));
                    }
                else
                    foreach (var entry in TMSStorage.GetApplicationRows())
                        rawApplications.Add(new Native.Application(entry));

                foreach (var entry in TMSStorage.GetAcquirerRows()) acquirers.Add(new Native.Acquirer(entry));

                foreach (var entry in TMSStorage.GetRiskManagementRows())
                    riskProfiles.Add(new Native.RiskManagement(entry));

                var error = Native.ProcessPayment(_nativeMpos, amount, rawApplications.ToArray(), rawApplications.Count,
                    acquirers.ToArray(), acquirers.Count, riskProfiles.ToArray(), riskProfiles.Count,
                    (int) magstripePaymentMethod, callback);

                if (error != Native.Error.Ok)
                    throw new MposException(error);

                return Native.Error.Ok;
            };
            tablePin = GCHandle.Alloc(tableCallback);

            var versionPin = default(GCHandle);
            Native.MposGetTableVersionCallbackDelegate versionCallback = (mpos, err, version) =>
            {
                versionPin.Free();

                var cleanVersionBytes = new byte[10];
                for (var i = 0; i < 10; i++)
                {
                    var pointer = IntPtr.Add(version, i * Marshal.SizeOf(typeof(byte)));
                    cleanVersionBytes[i] = (byte) Marshal.PtrToStructure(pointer, typeof(byte));
                }
                var cleanVersion = GetString(cleanVersionBytes);

                if (!TMSStorage.GetGlobalVersion().StartsWith(cleanVersion))
                {
                    var aidEntries = TMSStorage.GetAidRows();
                    var capkEntries = TMSStorage.GetCapkRows();

                    var tablePointer = Marshal.AllocHGlobal(IntPtr.Size * (aidEntries.Length + capkEntries.Length));
                    var offset = 0;

                    for (var i = 0; i < aidEntries.Length; i++)
                    {
                        var nativeAid = new Native.Aid(aidEntries[i]);
                        var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Native.Aid)));
                        Marshal.StructureToPtr(nativeAid, ptr, false);

                        Marshal.StructureToPtr(ptr, IntPtr.Add(tablePointer, offset * Marshal.SizeOf(typeof(IntPtr))),
                            false);
                        offset++;
                    }
                    for (var i = 0; i < capkEntries.Length; i++)
                    {
                        var nativeCapk = new Native.Capk(capkEntries[i]);
                        var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Native.Capk)));
                        Marshal.StructureToPtr(nativeCapk, ptr, false);

                        Marshal.StructureToPtr(ptr, IntPtr.Add(tablePointer, offset * Marshal.SizeOf(typeof(IntPtr))),
                            false);
                        offset++;
                    }

                    var updateError = Native.UpdateTables(mpos, tablePointer, aidEntries.Length + capkEntries.Length,
                        TMSStorage.GetGlobalVersion(), true, tableCallback);
                    if (updateError != Native.Error.Ok) throw new MposException(updateError);

                    for (var i = 0; i < aidEntries.Length + capkEntries.Length; i++)
                    {
                        var pointer = IntPtr.Add(tablePointer, i * Marshal.SizeOf(typeof(IntPtr)));
                        var deref = (IntPtr) Marshal.PtrToStructure(pointer, typeof(IntPtr));

                        Marshal.FreeHGlobal(deref);
                    }
                    Marshal.FreeHGlobal(tablePointer);
                }
                else
                {
                    tableCallback(_nativeMpos, 0, false);
                }

                return Native.Error.Ok;
            };
            versionPin = GCHandle.Alloc(versionCallback);

            var tableVersionError = Native.GetTableVersion(_nativeMpos, versionCallback);
            if (tableVersionError != Native.Error.Ok)
                throw new MposException(tableVersionError);

            return source.Task;
        }

        public Task FinishTransaction(bool success, int responseCode, string emvData)
        {
            var pin = default(GCHandle);
            var source = new TaskCompletionSource<bool>();

            Native.TransactionStatus status;
            int length;

            if (!success)
            {
                status = Native.TransactionStatus.Error;
                emvData = "";
                length = 0;
                responseCode = 0;
            }
            else
            {
                length = emvData == null ? 0 : emvData.Length;

                if (responseCode < 1000)
                    status = responseCode == 0 ? Native.TransactionStatus.Ok : Native.TransactionStatus.NonZero;
                else
                    status = Native.TransactionStatus.Error;
            }

            Native.MposFinishTransactionCallbackDelegate callback = (mpos, err) =>
            {
                pin.Free();

                OnFinishedTransaction(err);
                source.SetResult(true);

                return Native.Error.Ok;
            };

            pin = GCHandle.Alloc(callback);

            var error = Native.FinishTransaction(_nativeMpos, status, responseCode, emvData, length, callback);

            if (error != Native.Error.Ok)
                throw new MposException(error);

            return source.Task;
        }


        public void Display(string text)
        {
            var error = Native.Display(_nativeMpos, text);

            if (error != Native.Error.Ok)
                throw new MposException(error);
        }

        public void Cancel()
        {
            Native.Cancel(_nativeMpos);
        }

        public Task Close()
        {
            var pin = default(GCHandle);
            var source = new TaskCompletionSource<bool>();

            Native.MposClosedCallbackDelegate callback = (mpos, err) =>
            {
                pin.Free();

                OnClosed(err);
                source.SetResult(true);

                return Native.Error.Ok;
            };

            pin = GCHandle.Alloc(callback);

            var error = Native.Close(_nativeMpos, "", callback);

            if (error != Native.Error.Ok)
                throw new MposException(error);

            return source.Task;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }

            if (_nativeMpos != IntPtr.Zero)
                Native.Free(_nativeMpos);
        }

        protected virtual void OnInitialized(int error)
        {
            if (error != 0)
                Errored(this, error);
            else if (Initialized != null)
                Initialized(this, new EventArgs());
        }

        protected virtual void OnClosed(int error)
        {
            if (error != 0)
                Errored(this, error);
            else if (Closed != null)
                Closed(this, new EventArgs());
        }

        protected virtual void OnPaymentProcessed(PaymentResult result, int error)
        {
            if (error != 0)
                Errored(this, error);
            else if (PaymentProcessed != null)
                PaymentProcessed(this, result);
        }


        protected virtual void OnTableUpdated(bool loaded, int error)
        {
            if (error != 0)
                Errored(this, error);
            else if (TableUpdated != null)
                TableUpdated(this, loaded);
        }

        protected virtual void OnFinishedTransaction(int error)
        {
            if (error != 0)
                Errored(this, error);
            else if (FinishedTransaction != null)
                FinishedTransaction(this, new EventArgs());
        }

        private async Task<PaymentResult> HandlePaymentCallback(int error, Native.PaymentInfo info)
        {
            var result = new PaymentResult();

            if (error == 0)
            {
                var captureMethod = info.CaptureMethod == Native.CaptureMethod.EMV
                    ? CaptureMethod.EMV
                    : CaptureMethod.Magstripe;
                var status = info.Decision == Native.Decision.Refused ? PaymentStatus.Rejected : PaymentStatus.Accepted;
                var paymentMethod = (PaymentMethod) info.ApplicationType;
                var emv = captureMethod == CaptureMethod.EMV ? GetString(info.EmvData, info.EmvDataLength) : null;
                var pan = GetString(info.Pan, info.PanLength);
                var expirationDate = GetString(info.ExpirationDate);
                var holderName = info.HolderNameLength.ToInt32() > 0
                    ? GetString(info.HolderName, info.HolderNameLength)
                    : null;
                var panSequenceNumber = info.PanSequenceNumber;
                string pin = null, pinKek = null;
                var isOnlinePin = info.IsOnlinePin != 0;
                var requiredPin = info.PinRequired != 0;

                var track1 = info.Track1Length.ToInt32() > 0 ? GetString(info.Track1, info.Track1Length) : null;
                var track2 = GetString(info.Track2, info.Track2Length);
                var track3 = info.Track3Length.ToInt32() > 0 ? GetString(info.Track3, info.Track3Length) : null;

                expirationDate = expirationDate.Substring(2, 2) + expirationDate.Substring(0, 2);
                if (holderName != null)
                    holderName = holderName.Trim().Split('/').Reverse().Aggregate((a, b) => a + ' ' + b);

                if (requiredPin && isOnlinePin)
                {
                    pin = GetString(info.Pin);
                    pinKek = GetString(info.PinKek);
                }

                await result.BuildAccepted(EncryptionKey, status, captureMethod, paymentMethod, pan, holderName,
                    expirationDate, panSequenceNumber, track1, track2, track3, emv, isOnlinePin, requiredPin, pin,
                    pinKek);
            }
            else
            {
                result.BuildErrored();
            }

            return result;
        }


        private void HandleNotificationCallback(IntPtr mpos, string notification)
        {
            if (NotificationReceived != null)
                NotificationReceived(this, notification);
        }

        private void HandleOperationCompletedCallback(IntPtr mpos)
        {
            if (OperationCompleted != null)
                OperationCompleted(this, new EventArgs());
        }
    }
}