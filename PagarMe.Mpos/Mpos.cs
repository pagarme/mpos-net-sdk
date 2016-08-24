using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Linq;

namespace PagarMe.Mpos
{
	public class Mpos : IDisposable
	{
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

		private static IntPtr GetMarshalBytes<T>(T str) {
			int size = Marshal.SizeOf(typeof(T));

			IntPtr ptr = Marshal.AllocHGlobal(size);

			Marshal.StructureToPtr(str, ptr, false);

			return ptr;
		}

		private AbecsStream _stream;
		private IntPtr _nativeMpos;
		private TMSStorage _tmsStorage;
		private readonly string _encryptionKey;
		private readonly string _storagePath;

		private Native.MposNotificationCallbackDelegate NotificationPin;
		private Native.MposOperationCompletedCallbackDelegate OperationPin;

		public event EventHandler<int> Errored;
		public event EventHandler Initialized;
		public event EventHandler Closed;
		public event EventHandler<PaymentResult> PaymentProcessed;
		public event EventHandler<bool> TableUpdated;
		public event EventHandler FinishedTransaction;
		public event EventHandler<string> NotificationReceived;
		public event EventHandler OperationCompleted;

		public Stream BaseStream { get { return _stream.BaseStream; } }
		public string EncryptionKey { get { return _encryptionKey; } }
		public string StoragePath { get { return _storagePath; } }
		public TMSStorage TMSStorage { get { return _tmsStorage; } }

		public Mpos(Stream stream, string encryptionKey, string storagePath)
			: this(new AbecsStream(stream), encryptionKey, storagePath)
		{
		}

		private unsafe Mpos(AbecsStream stream, string encryptionKey, string storagePath)
		{
			NotificationPin = HandleNotificationCallback;
			OperationPin = HandleOperationCompletedCallback;

			_stream = stream;
			_encryptionKey = encryptionKey;
			_storagePath = storagePath;
			_nativeMpos = Native.Create((IntPtr)stream.NativeStream, NotificationPin, OperationPin);
			_tmsStorage = new TMSStorage(storagePath, encryptionKey);
		}

		~Mpos()
		{
			Dispose(false);
		}

		public Task Initialize()
		{
			GCHandle pin = default(GCHandle);
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

			Native.Error error = Native.Initialize(_nativeMpos, IntPtr.Zero, callback);

			if (error != Native.Error.Ok)
				throw new MposException(error);

			return source.Task;
		}

		public Task SynchronizeTables(bool forceUpdate)
		{
			var source = new TaskCompletionSource<bool>();
			GCHandle keysPin = default(GCHandle);

			Native.MposExtractKeysCallbackDelegate keysCallback = (mpos, err, keys, keyLength) => {
				GCHandle pin = default(GCHandle);

				Native.TmsStoreCallbackDelegate tmsCallback = (version, tables, tableLen, applications, appLen, riskProfiles, riskLen, acquirers, acqLen, userData) => {
					pin.Free();

					GCHandle tablePin = default(GCHandle);
					
					Native.MposTablesLoadedCallbackDelegate callback = (mpos2, tableError, loaded) => {
						tablePin.Free();

						OnTableUpdated(loaded, tableError);
						source.SetResult(true);

						return Native.Error.Ok;
					};
					tablePin = GCHandle.Alloc(callback);

					this.TMSStorage.PurgeIndex();
					this.TMSStorage.StoreGlobalVersion(version);
					
					for (int i = 0; i < tableLen; i++) {
						IntPtr pointer = IntPtr.Add(tables, i * Marshal.SizeOf(typeof(IntPtr)));
						IntPtr deref = (IntPtr)Marshal.PtrToStructure(pointer, typeof(IntPtr));

						// We assume everything is the smaller member
						var capk = (Native.Capk)Marshal.PtrToStructure(deref, typeof(Native.Capk));
						var isAid = capk.IsAid;

						if (isAid) {
							var aid = (Native.Aid)Marshal.PtrToStructure(deref, typeof(Native.Aid));
							this.TMSStorage.StoreAidRow(aid.AcquirerNumber, aid.RecordIndex, aid.AidLength, aid.AidNumber, aid.ApplicationType, aid.ApplicationNameLength, aid.ApplicationName, aid.AppVersion1, aid.AppVersion2, aid.AppVersion3, aid.CountryCode, aid.Currency, aid.CurrencyExponent, aid.MerchantId, aid.Mcc, aid.TerminalId, aid.TerminalCapabilities, aid.AdditionalTerminalCapabilities, aid.TerminalType, aid.DefaultTac, aid.DenialTac, aid.OnlineTac, aid.FloorLimit, aid.Tcc, aid.CtlsZeroAm, aid.CtlsMode, aid.CtlsTransactionLimit, aid.CtlsFloorLimit, aid.CtlsCvmLimit, aid.CtlsApplicationVersion, aid.TdolLength, aid.Tdol, aid.DdolLength, aid.Ddol);
						}
						else {
							this.TMSStorage.StoreCapkRow(capk.AcquirerNumber, capk.RecordIndex, capk.Rid, capk.CapkIndex, capk.ExponentLength, capk.Exponent, capk.ModulusLength, capk.Modulus, capk.HasChecksum, capk.Checksum);
						}
					}
					
					for (int i = 0; i < appLen; i++) {
						IntPtr pointer = IntPtr.Add(applications, i * Marshal.SizeOf(typeof(IntPtr)));
						IntPtr deref = (IntPtr)Marshal.PtrToStructure(pointer, typeof(IntPtr));

						var app = (Native.Application)Marshal.PtrToStructure(deref, typeof(Native.Application));
						
						this.TMSStorage.StoreApplicationRow(app.PaymentMethod, app.CardBrand, app.AcquirerNumber, app.RecordNumber, app.EmvTagsLength, app.EmvTags);
					}
					
					for (int i = 0; i < appLen; i++) {
						IntPtr pointer = IntPtr.Add(riskProfiles, i * Marshal.SizeOf(typeof(IntPtr)));
						IntPtr deref = (IntPtr)Marshal.PtrToStructure(pointer, typeof(IntPtr));

						var profile = (Native.RiskManagement)Marshal.PtrToStructure(deref, typeof(Native.RiskManagement));

						this.TMSStorage.StoreRiskManagementRow(profile.AcquirerNumber, profile.RecordNumber, profile.MustRiskManagement, profile.FloorLimit, profile.BiasedRandomSelectionPercentage, profile.BiasedRandomSelectionThreshold, profile.BiasedRandomSelectionMaxPercentage);
					}

					for (int i = 0; i < acqLen; i++) {
						IntPtr pointer = IntPtr.Add(acquirers, i * Marshal.SizeOf(typeof(IntPtr)));
						IntPtr deref = (IntPtr)Marshal.PtrToStructure(pointer, typeof(IntPtr));
						
						var acquirer = (Native.Acquirer)Marshal.PtrToStructure(deref, typeof(Native.Acquirer));

						this.TMSStorage.StoreAcquirerRow(acquirer.Number, acquirer.CryptographyMethod, acquirer.KeyIndex, acquirer.SessionKey);
					}

					Native.Error updateError = Native.UpdateTables(_nativeMpos, tables, tableLen, version, forceUpdate, callback);
					if (updateError != Native.Error.Ok) {
						throw new MposException(updateError);
					}
					
					return updateError;
				};
				pin = GCHandle.Alloc(tmsCallback);
				
				int[] cleanKeys = new int[keyLength];
				for (int i = 0; i < keyLength; i++) {
					IntPtr pointer = IntPtr.Add(keys, i * Marshal.SizeOf(typeof(int)));
					cleanKeys[i] = (int)Marshal.PtrToStructure(pointer, typeof(int));
				}
				
				ApiHelper.GetTerminalTables(this.EncryptionKey, !forceUpdate ? this.TMSStorage.GetGlobalVersion() : "", cleanKeys).ContinueWith(t => {
					if (t.Status == TaskStatus.Faulted) {
						source.SetException(t.Exception);
						return;
					}
					
					if (t.Result.Length > 0) {
						Native.Error error = Native.TmsGetTables(t.Result, t.Result.Length, tmsCallback, IntPtr.Zero);
						if (error != Native.Error.Ok) {
							throw new MposException(error);					
						}
					}

					else {
						// We don't need to do anything; complete operation.	
						OnTableUpdated(false, 0);
						source.SetResult(true);						
					}
				});

				return Native.Error.Ok;
			};
			keysPin = GCHandle.Alloc(keysCallback);

			Native.Error keysError = Native.ExtractKeys(_nativeMpos, keysCallback);
			if (keysError != Native.Error.Ok) {
				throw new MposException(keysError);
			}

			return source.Task;
		}

		public Task<PaymentResult> ProcessPayment(int amount, List<EmvApplication> applications = null, PaymentMethod magstripePaymentMethod = PaymentMethod.Credit)
		{
			var source = new TaskCompletionSource<PaymentResult>();

			GCHandle tablePin = default(GCHandle);
			Native.MposTablesLoadedCallbackDelegate tableCallback = (mpos2, tableError, loaded) => {
				tablePin.Free();

				GCHandle pin = default(GCHandle);
				Native.MposPaymentCallbackDelegate callback = (mpos, err, infoPointer) => {
					pin.Free();
					
					if (err != 0) {
						OnPaymentProcessed(null, err);
						return Native.Error.Ok;
					}
					var info = (Native.PaymentInfo)Marshal.PtrToStructure(infoPointer, typeof(Native.PaymentInfo));

					HandlePaymentCallback(err, info).ContinueWith(t => {
						if (t.Status == TaskStatus.Faulted) {
							source.SetException(t.Exception);
						}
						else {
							source.SetResult(t.Result);
						}

						OnPaymentProcessed(t.Result, err);
					});

					return Native.Error.Ok;
				};
				pin = GCHandle.Alloc(callback);

				List<Native.Acquirer> acquirers = new List<Native.Acquirer>();
				List<Native.RiskManagement> riskProfiles = new List<Native.RiskManagement>();

				List<Native.Application> rawApplications = new List<Native.Application>();
				if (applications != null) {
					foreach (EmvApplication application in applications) {
						ApplicationEntry entry = this.TMSStorage.SelectApplication(application.Brand, (int)application.PaymentMethod);
						if (entry != null) {
							rawApplications.Add(new Native.Application(entry));
						}
					}
				}
				else {
					foreach (ApplicationEntry entry in this.TMSStorage.GetApplicationRows()) {
						rawApplications.Add(new Native.Application(entry));
					}
				}

				foreach (AcquirerEntry entry in this.TMSStorage.GetAcquirerRows()) {
					acquirers.Add(new Native.Acquirer(entry));
				}
				
				foreach (RiskManagementEntry entry in this.TMSStorage.GetRiskManagementRows()) {
					riskProfiles.Add(new Native.RiskManagement(entry));
				}

				Native.Error error = Native.ProcessPayment(_nativeMpos, amount, rawApplications.ToArray(), rawApplications.Count, acquirers.ToArray(), acquirers.Count, riskProfiles.ToArray(), riskProfiles.Count, (int)magstripePaymentMethod, callback);

				if (error != Native.Error.Ok)
					throw new MposException(error);

				return Native.Error.Ok;
			};
			tablePin = GCHandle.Alloc(tableCallback);					
			
			GCHandle versionPin = default(GCHandle);
			Native.MposGetTableVersionCallbackDelegate versionCallback = (mpos, err, version) => {
				versionPin.Free();
				
				byte[] cleanVersionBytes = new byte[10];
				for (int i = 0; i < 10; i++) {
					IntPtr pointer = IntPtr.Add(version, i * Marshal.SizeOf(typeof(byte)));
					cleanVersionBytes[i] = (byte)Marshal.PtrToStructure(pointer, typeof(byte));
				}
				string cleanVersion = GetString(cleanVersionBytes);

				if (!this.TMSStorage.GetGlobalVersion().StartsWith(cleanVersion)) {
					AidEntry[] aidEntries = this.TMSStorage.GetAidRows();
					CapkEntry[] capkEntries = this.TMSStorage.GetCapkRows();
					
					IntPtr tablePointer = Marshal.AllocHGlobal(IntPtr.Size * (aidEntries.Length + capkEntries.Length));
					int offset = 0;
					
					for (int i = 0; i < aidEntries.Length; i++) {
						Native.Aid nativeAid = new Native.Aid(aidEntries[i]);
						IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Native.Aid)));
						Marshal.StructureToPtr(nativeAid, ptr, false);

						Marshal.StructureToPtr(ptr, IntPtr.Add(tablePointer, offset * Marshal.SizeOf(typeof(IntPtr))), false);
						offset++;
					}
					for (int i = 0; i < capkEntries.Length; i++) {
						Native.Capk nativeCapk = new Native.Capk(capkEntries[i]);
						IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Native.Capk)));
						Marshal.StructureToPtr(nativeCapk, ptr, false);

						Marshal.StructureToPtr(ptr, IntPtr.Add(tablePointer, offset * Marshal.SizeOf(typeof(IntPtr))), false);						
						offset++;
					}
					
					Native.Error updateError = Native.UpdateTables(mpos, tablePointer, aidEntries.Length + capkEntries.Length, this.TMSStorage.GetGlobalVersion(), true, tableCallback);
					if (updateError != Native.Error.Ok) {
						throw new MposException(updateError);
					}					

					for (int i = 0; i < (aidEntries.Length + capkEntries.Length); i++) {
						IntPtr pointer = IntPtr.Add(tablePointer, i * Marshal.SizeOf(typeof(IntPtr)));
						IntPtr deref = (IntPtr)Marshal.PtrToStructure(pointer, typeof(IntPtr));						
						
						Marshal.FreeHGlobal(deref);
					}
					Marshal.FreeHGlobal(tablePointer);
				}
				else {
					tableCallback(_nativeMpos, 0, false);
				}

				return Native.Error.Ok;
			};
			versionPin = GCHandle.Alloc(versionCallback);

			Native.Error tableVersionError = Native.GetTableVersion(_nativeMpos, versionCallback);
			if (tableVersionError != Native.Error.Ok)
				throw new MposException(tableVersionError);

			return source.Task;
		}

		public Task FinishTransaction(bool success, int responseCode, string emvData)
		{
			GCHandle pin = default(GCHandle);
			var source = new TaskCompletionSource<bool>();

			Native.TransactionStatus status;
			int length;

			if (!success) {
				status = Native.TransactionStatus.Error;
				emvData = "";
				length = 0;
				responseCode = 0;
			}
			else {
				length = emvData == null ? 0 : emvData.Length;
				
				if (responseCode < 1000)
				{
					status = responseCode == 0 ? Native.TransactionStatus.Ok : Native.TransactionStatus.NonZero;
				}
				else
				{
					status = Native.TransactionStatus.Error;
				}
			}

			Native.MposFinishTransactionCallbackDelegate callback = (mpos, err) =>
			{
				pin.Free();

				OnFinishedTransaction(err);
				source.SetResult(true);

				return Native.Error.Ok;
			};

			pin = GCHandle.Alloc(callback);

			Native.Error error = Native.FinishTransaction(_nativeMpos, status, responseCode, emvData, length, callback);

			if (error != Native.Error.Ok)
				throw new MposException(error);

			return source.Task;
		}


		public void Display(string text)
		{
			Native.Error error = Native.Display(_nativeMpos, text);

			if (error != Native.Error.Ok)
				throw new MposException(error);
		}

		public Task Close()
		{
			GCHandle pin = default(GCHandle);
			var source = new TaskCompletionSource<bool>();

			Native.MposClosedCallbackDelegate callback = (mpos, err) =>
			{
				pin.Free();

				OnClosed(err);
				source.SetResult(true);

				return Native.Error.Ok;
			};

			pin = GCHandle.Alloc(callback);

			Native.Error error = Native.Close(_nativeMpos, "", callback);

			if (error != Native.Error.Ok)
				throw new MposException(error);

			return source.Task;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_stream != null)
				{
					_stream.Dispose();
					_stream = null;
				}
			}

			if (_nativeMpos != IntPtr.Zero)
			{
				Native.Free(_nativeMpos);
			}
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


		protected virtual void OnTableUpdated(bool loaded, int error) {
			if (error != 0)
				Errored(this, error);
			else if (TableUpdated != null)
				TableUpdated(this, loaded);
		}

		protected virtual void OnFinishedTransaction(int error) {
			if (error != 0)
				Errored(this, error);
			else if (FinishedTransaction != null)
				FinishedTransaction(this, new EventArgs());
		}

		private async Task<PaymentResult> HandlePaymentCallback(int error, Native.PaymentInfo info)
		{
			PaymentResult result = new PaymentResult();

			if (error == 0)
			{
				CaptureMethod captureMethod = info.CaptureMethod == Native.CaptureMethod.EMV ? CaptureMethod.EMV : CaptureMethod.Magstripe;
				PaymentStatus status = info.Decision == Native.Decision.Refused ? PaymentStatus.Rejected : PaymentStatus.Accepted;
				PaymentMethod paymentMethod = (PaymentMethod)info.ApplicationType;
				string emv = captureMethod == CaptureMethod.EMV ? GetString(info.EmvData, info.EmvDataLength) : null;
				string pan = GetString(info.Pan, info.PanLength);
				string expirationDate = GetString(info.ExpirationDate);
				string holderName = info.HolderNameLength.ToInt32() > 0 ?  GetString(info.HolderName, info.HolderNameLength) : null;
				string pin = null, pinKek = null;
				bool isOnlinePin = info.IsOnlinePin != 0;
				bool requiredPin = info.PinRequired != 0;

				string track1 = info.Track1Length.ToInt32() > 0 ? GetString(info.Track1, info.Track1Length) : null;
				string track2 = GetString(info.Track2, info.Track2Length);
				string track3 = info.Track3Length.ToInt32() > 0 ? GetString(info.Track3, info.Track3Length) : null;

				expirationDate = expirationDate.Substring(2, 2) + expirationDate.Substring(0, 2);
				if (holderName != null)
					holderName = holderName.Trim().Split('/').Reverse().Aggregate((a, b) => a + ' ' + b);

				if (requiredPin && isOnlinePin)
				{
					pin = GetString(info.Pin);
					pinKek = GetString(info.PinKek);
				}

				await result.BuildAccepted(this.EncryptionKey, status, captureMethod, paymentMethod, pan, holderName, expirationDate, track1, track2, track3, emv, isOnlinePin, requiredPin, pin, pinKek);
			}
			else
			{
				result.BuildErrored();
			}

			return result;
		}

		private unsafe void HandleNotificationCallback(IntPtr mpos, string notification)
		{
			if (NotificationReceived != null)
				NotificationReceived(this, notification);
		}

		private unsafe void HandleOperationCompletedCallback(IntPtr mpos)
		{
			if (OperationCompleted != null)
				OperationCompleted(this, new EventArgs());
		}

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
					public unsafe struct Acquirer
					{
						public int Number;
						public int CryptographyMethod;
						public int KeyIndex;
						
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
							public byte[] SessionKey;

						public Acquirer(AcquirerEntry e) {
							Number = e.Number;
							CryptographyMethod = e.CryptographyMethod;
							KeyIndex = e.KeyIndex;

							if (e.SessionKey != null) SessionKey = GetHexBytes(e.SessionKey, 32);
							else SessionKey = GetHexBytes("", 32);
						}
					}

				[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
					public unsafe struct RiskManagement
					{
						public int AcquirerNumber;
						public int RecordNumber;

						[MarshalAs(UnmanagedType.I1)]
							public bool MustRiskManagement;
						public int FloorLimit;
						public int BiasedRandomSelectionPercentage;
						public int BiasedRandomSelectionThreshold;
						public int BiasedRandomSelectionMaxPercentage;

						public RiskManagement(RiskManagementEntry e) {
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
					public unsafe struct Application {
						public int PaymentMethod;
						[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
							public string CardBrand;

						public int AcquirerNumber;
						public int RecordNumber;
						
						public int EmvTagsLength;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
							public int[] EmvTags;

						public Application(ApplicationEntry e) {
							PaymentMethod = e.PaymentMethod;
							CardBrand = e.CardBrand;

							AcquirerNumber = e.AcquirerNumber;
							RecordNumber = e.RecordNumber;

							EmvTags = e.EmvTags.Split(',').Select(int.Parse).ToArray();
							EmvTagsLength = EmvTags.Length;
						}
					}
				
				[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
					public unsafe struct PaymentInfo
					{
						[MarshalAs(UnmanagedType.I4)]
							public CaptureMethod CaptureMethod;

						[MarshalAs(UnmanagedType.I4)]
							public Decision Decision;

						public int Amount;
						public int AcquirerIndex;
						public int RecordNumber;
						public int ApplicationType;

						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
							public byte[] ExpirationDate;

						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
							public byte[] HolderName;
						public IntPtr HolderNameLength;

						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
							public byte[] Pan;
						public IntPtr PanLength;

						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 76)]
							public byte[] Track1;
						public IntPtr Track1Length;

						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
							public byte[] Track2;
						public IntPtr Track2Length;

						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 104)]
							public byte[] Track3;
						public IntPtr Track3Length;

						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
							public byte[] EmvData;
						public IntPtr EmvDataLength;

						public int PinRequired;
						public int IsOnlinePin;

						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
							public byte[] Pin;

						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
							public byte[] PinKek;
					}

				 [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
					public unsafe struct Capk
					{
						[MarshalAs(UnmanagedType.I1)]
							public bool IsAid;
						public int AcquirerNumber;
						public int RecordIndex;

						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
							public byte[] Rid;
						public int CapkIndex;
						public int ExponentLength;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
							public byte[] Exponent;
						public int ModulusLength;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 496)]
							public byte[] Modulus;

						[MarshalAs(UnmanagedType.I1)]
							public bool HasChecksum;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
							public byte[] Checksum;

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
					public unsafe struct Aid
					{
						[MarshalAs(UnmanagedType.I1)]
							public bool IsAid;
						public int AcquirerNumber;
						public int RecordIndex;

						public int AidLength;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
							public byte[] AidNumber;
						public int ApplicationType;
						public int ApplicationNameLength;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
							public byte[] ApplicationName;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
							public byte[] AppVersion1;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
							public byte[] AppVersion2;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
							public byte[] AppVersion3;
						public int CountryCode;
						public int Currency;
						public int CurrencyExponent;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
							public byte[] MerchantId;
						public int Mcc;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
							public byte[] TerminalId;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
							public byte[] TerminalCapabilities;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
							public byte[] AdditionalTerminalCapabilities;
						public int TerminalType;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
							public byte[] DefaultTac;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
							public byte[] DenialTac;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
							public byte[] OnlineTac;
						public int FloorLimit;
						[MarshalAs(UnmanagedType.I1)]
							public byte Tcc;

						[MarshalAs(UnmanagedType.I1)]
							public bool CtlsZeroAm;
						public int CtlsMode;
						public int CtlsTransactionLimit;
						public int CtlsFloorLimit;
						public int CtlsCvmLimit;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
							public byte[] CtlsApplicationVersion;

						public int TdolLength;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
							public byte[] Tdol;
						public int DdolLength;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
							public byte[] Ddol;

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

				public static byte[] GetBytes(string data, int length, out int newSize, char? fill = null, bool padLeft = true)
				{
					newSize = Encoding.UTF8.GetByteCount(data);

					if (fill.HasValue && data.Length < length)
						data = padLeft ? data.PadLeft(length, fill.Value) : data.PadRight(length, fill.Value);

					byte[] result = Encoding.UTF8.GetBytes(data);
					byte[] full = new byte[length];

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
					byte[] result = GetBytes(data, length, out byteLength, '0', padLeft);

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
					return GetHexBytes(data.Select(x => x.ToString("X2")).Aggregate((a, b) => a + b), length, out byteLength, padLeft);
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
					public delegate Error MposPaymentCallbackDelegate(IntPtr mpos, int error, IntPtr info);

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
					public delegate Error MposTablesLoadedCallbackDelegate(IntPtr mpos, int error, bool loaded);

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
					public delegate Error MposFinishTransactionCallbackDelegate(IntPtr mpos, int error);

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
					public delegate Error MposExtractKeysCallbackDelegate(IntPtr mpos, int error, IntPtr keys, int keysLength);

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
					public delegate Error MposGetTableVersionCallbackDelegate(IntPtr mpos, int error, IntPtr version);

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
					public delegate Error MposClosedCallbackDelegate(IntPtr mpos, int error);

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
					public delegate Error TmsStoreCallbackDelegate(string version, IntPtr tables, int tableLen, IntPtr applications, int appLen, IntPtr riskManagement, int riskmanLen, IntPtr acquirers, int acqLen, IntPtr userData);

				[DllImport("mpos", EntryPoint = "mpos_new", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern IntPtr Create(IntPtr stream, MposNotificationCallbackDelegate notificationCallback, MposOperationCompletedCallbackDelegate operationCompletedCallback);

				[DllImport("mpos", EntryPoint = "mpos_initialize", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error Initialize(IntPtr mpos, IntPtr streamData, MposInitializedCallbackDelegate initializedCallback);

				[DllImport("mpos", EntryPoint = "mpos_process_payment", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error ProcessPayment(IntPtr mpos, int amount, Native.Application[] applicationList, int applicationListLength, Native.Acquirer[] acquirers, int acquirerListLength,Native.RiskManagement[] riskManagementList, int riskManagementListLength, int magstripePaymentMethod, MposPaymentCallbackDelegate paymentCallback);

				[DllImport("mpos", EntryPoint = "mpos_update_tables", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error UpdateTables(IntPtr mpos, IntPtr data, int count, string version, bool force_update, MposTablesLoadedCallbackDelegate callback);

				[DllImport("mpos", EntryPoint = "mpos_finish_transaction", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error FinishTransaction(IntPtr mpos, TransactionStatus status, int arc, string emv, int emvLen, MposFinishTransactionCallbackDelegate callback);

				[DllImport("mpos", EntryPoint = "mpos_extract_keys", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error ExtractKeys(IntPtr mpos, MposExtractKeysCallbackDelegate callback);

				[DllImport("mpos", EntryPoint = "mpos_get_table_version", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error GetTableVersion(IntPtr mpos, MposGetTableVersionCallbackDelegate callback);

				[DllImport("mpos", EntryPoint = "mpos_display", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error Display(IntPtr mpos, string text);

				[DllImport("mpos", EntryPoint = "mpos_close", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error Close(IntPtr mpos, string text, MposClosedCallbackDelegate callback);

				[DllImport("mpos", EntryPoint = "mpos_free", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error Free(IntPtr mpos);
				
				[DllImport("tms", EntryPoint = "tms_get_tables", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error TmsGetTables(string payload, int length, TmsStoreCallbackDelegate callback, IntPtr userData);
			}
	}
}

