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
		private readonly string _apiKey;
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
		public string ApiKey { get { return _apiKey; } }
		public string EncryptionKey { get { return _encryptionKey; } }
		public string StoragePath { get { return _storagePath; } }
		public TMSStorage TMSStorage { get { return _tmsStorage; } }

		public Mpos(Stream stream, string apiKey, string encryptionKey, string storagePath)
			: this(new AbecsStream(stream), apiKey, encryptionKey, storagePath)
		{
		}

		private unsafe Mpos(AbecsStream stream, string apiKey, string encryptionKey, string storagePath)
		{
			NotificationPin = HandleNotificationCallback;
			OperationPin = HandleOperationCompletedCallback;

			_stream = stream;
			_apiKey = apiKey;
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

			pin = GCHandle.Alloc(pin);

			Native.Error error = Native.Initialize(_nativeMpos, IntPtr.Zero, callback);

			if (error != Native.Error.Ok)
				throw new MposException(error);

			return source.Task;
		}

		public Task SynchronizeTables(bool forceUpdate)
		{
			GCHandle pin = default(GCHandle);
			var source = new TaskCompletionSource<bool>();

			Native.TmsStoreCallbackDelegate tmsCallback = (string version, IntPtr[] tables, Native.Application[] applications, Native.Acquirer[] acquirers, Native.RiskManagement[] riskProfiles) => {
				pin.Free();

				GCHandle tablePin = default(GCHandle);
				
				Native.MposTablesLoadedCallbackDelegate callback = (mpos, err, loaded) => {
					tablePin.Free();

					OnTableUpdated(loaded, err);
					source.SetResult(true);

					return Native.Error.Ok;
				};
				
				foreach (Native.Application application in applications) {
					this.TMSStorage.StoreApplicationRow(application.PaymentMethod, application.CardBrand, application.AcquirerNumber, application.RecordNumber);
				}
				foreach (Native.Acquirer acquirer in acquirers) {
					this.TMSStorage.StoreAcquirerRow(acquirer.Number, acquirer.CryptographyMethod, acquirer.KeyIndex, acquirer.SessionKey, acquirer.EmvTagsLength, acquirer.EmvTags);
				}
				foreach (Native.RiskManagement riskManagement in riskProfiles) {
					this.TMSStorage.StoreRiskManagementRow(riskManagement.AcquirerNumber, riskManagement.RecordNumber, riskManagement.MustRiskManagement, riskManagement.FloorLimit, riskManagement.BiasedRandomSelectionPercentage, riskManagement.BiasedRandomSelectionThreshold, riskManagement.BiasedRandomSelectionMaxPercentage);
				}

				Native.Error error = Native.UpdateTables(_nativeMpos, tables, tables.Length, version, forceUpdate, callback);
				return error;
			};
			
			ApiHelper.GetTerminalTables(this.ApiKey).ContinueWith(t => {
				pin = GCHandle.Alloc(tmsCallback);
				
				Native.Error error = Native.TmsGetTables(t.Result, t.Result.Length, tmsCallback);
				if (error != Native.Error.Ok) {
					throw new MposException(error);					
				}
			});

			return source.Task;
		}

		public Task<PaymentResult> ProcessPayment(int amount, List<EmvApplication> applications = null, PaymentMethod magstripePaymentMethod = PaymentMethod.Credit)
		{
			GCHandle pin = default(GCHandle);
			var source = new TaskCompletionSource<PaymentResult>();

			Native.MposPaymentCallbackDelegate callback = (mpos, err, infoPointer) =>
			{
				if (err != 0) {
					OnPaymentProcessed(null, err);
					return Native.Error.Ok;
				}
				var info = (Native.PaymentInfo)Marshal.PtrToStructure(infoPointer, typeof(Native.PaymentInfo));

				pin.Free();

				HandlePaymentCallback(err, info).ContinueWith(t =>
				{
					if (t.Status == TaskStatus.Faulted)
					{
						source.SetException(t.Exception);
					}
					else
					{
						source.SetResult(t.Result);
					}

					OnPaymentProcessed(t.Result, err);
				});

				return Native.Error.Ok;
			};

			pin = GCHandle.Alloc(callback);

			if (applications == null) {
				applications = new List<EmvApplication>();
				applications.Add(new EmvApplication("visa", PaymentMethod.Credit));
				applications.Add(new EmvApplication("visa", PaymentMethod.Debit));
				applications.Add(new EmvApplication("mastercard", PaymentMethod.Credit));
				applications.Add(new EmvApplication("mastercard", PaymentMethod.Debit));
			}
			
			List<Native.Acquirer> acquirers = new List<Native.Acquirer>();
			List<Native.RiskManagement> riskProfiles = new List<Native.RiskManagement>();

			List<Native.Application> rawApplications = new List<Native.Application>();
			foreach (EmvApplication application in applications) {
				TMSStorage.ApplicationEntry? entry = this.TMSStorage.SelectApplication(application.Brand, (int)application.PaymentMethod);
				if (entry != null) {
					rawApplications.Add(new Native.Application(entry.Value));
				}
			}

			foreach (TMSStorage.AcquirerEntry entry in this.TMSStorage.GetAcquirerRows()) {
				acquirers.Add(new Native.Acquirer(entry));
			}
			foreach (TMSStorage.RiskManagementEntry entry in this.TMSStorage.GetRiskManagementRows()) {
				riskProfiles.Add(new Native.RiskManagement(entry));
			}

			Native.Error error = Native.ProcessPayment(_nativeMpos, amount, rawApplications.Count, rawApplications.ToArray(), acquirers.Count, acquirers.ToArray(), riskProfiles.Count, riskProfiles.ToArray(), (int)magstripePaymentMethod, callback);

			if (error != Native.Error.Ok)
				throw new MposException(error);
			
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

			Native.Error error = Native.FinishTransaction(_nativeMpos, status, responseCode, length, emvData, callback);

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
					Ok,
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

						public int EmvTagsLength;
						[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
							public int[] EmvTags;

						public Acquirer(TMSStorage.AcquirerEntry e) {
							Number = e.Number;
							CryptographyMethod = e.CryptographyMethod;
							KeyIndex = e.KeyIndex;

							if (e.SessionKey != null) SessionKey = GetHexBytes(e.SessionKey, 32);
							else SessionKey = GetHexBytes("", 32);

							EmvTagsLength = e.EmvTags.Length;
							EmvTags = e.EmvTags;
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

						public RiskManagement(TMSStorage.RiskManagementEntry e) {
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
						public string CardBrand;

						public int AcquirerNumber;
						public int RecordNumber;

						public Application(TMSStorage.ApplicationEntry e) {
							PaymentMethod = e.PaymentMethod;
							CardBrand = e.CardBrand;

							AcquirerNumber = e.AcquirerNumber;
							RecordNumber = e.RecordNumber;
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
					public delegate Error MposClosedCallbackDelegate(IntPtr mpos, int error);

				[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
					public delegate Error TmsStoreCallbackDelegate(string version, IntPtr[] tables, Native.Application[] applications, Native.Acquirer[] acquirers, Native.RiskManagement[] riskManagement);

				[DllImport("mpos", EntryPoint = "mpos_new", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern IntPtr Create(IntPtr stream, MposNotificationCallbackDelegate notificationCallback, MposOperationCompletedCallbackDelegate operationCompletedCallback);

				[DllImport("mpos", EntryPoint = "mpos_initialize", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error Initialize(IntPtr mpos, IntPtr streamData, MposInitializedCallbackDelegate initializedCallback);

				[DllImport("mpos", EntryPoint = "mpos_process_payment", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error ProcessPayment(IntPtr mpos, int amount, int applicationListLength, Native.Application[] applicationList, int acquirerListLength, Native.Acquirer[] acquirers, int riskManagementListLength, Native.RiskManagement[] riskManagementList, int magstripePaymentMethod, MposPaymentCallbackDelegate paymentCallback);

				[DllImport("mpos", EntryPoint = "mpos_update_tables", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error UpdateTables(IntPtr mpos, IntPtr[] data, int count, string version, bool force_update, MposTablesLoadedCallbackDelegate callback);

				[DllImport("mpos", EntryPoint = "mpos_finish_transaction", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error FinishTransaction(IntPtr mpos, TransactionStatus status, int arc, int emvLen, string emv, MposFinishTransactionCallbackDelegate callback);

				[DllImport("mpos", EntryPoint = "mpos_display", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error Display(IntPtr mpos, string text);

				[DllImport("mpos", EntryPoint = "mpos_close", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error Close(IntPtr mpos, string text, MposClosedCallbackDelegate callback);

				[DllImport("mpos", EntryPoint = "mpos_free", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error Free(IntPtr mpos);
				
				[DllImport("tms", EntryPoint = "tms_get_tables", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
					public static extern Error TmsGetTables(string payload, int length, TmsStoreCallbackDelegate callback);
			}
	}
}

