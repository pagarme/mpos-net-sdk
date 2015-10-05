using System;
using System.IO;
using System.Threading.Tasks;

namespace PagarMe.Mpos
{
	public class Mpos
	{
		private bool _initialized;
		private readonly Stream _stream;
		private readonly string _encryptionKey;

		public event EventHandler Initialized;
		public event EventHandler<PaymentResult> PaymentProcessed;

		public Stream Stream { get { return _stream; } }
		public string EncryptionKey { get { return _encryptionKey; } }
		public bool IsInitialized { get { return _initialized; } }

		public Mpos(Stream stream, string encryptionKey)
		{
			_stream = stream;
			_encryptionKey = encryptionKey;
			_initialized = false;
		}

		public async Task Initialize()
		{
			OnInitialized();
		}

		public async Task<PaymentResult> ProcessPayment(int amount, PaymentFlags flags)
		{
			PaymentResult result;

			if (!_initialized)
				throw new InvalidOperationException("Device is not ready.");


			OnPaymentProcessed(result);

			return result;
		}

		public void Close()
		{

		}

		protected void OnPaymentProcessed(PaymentResult result)
		{
			if (PaymentProcessed != null)
				PaymentProcessed(this, result);
		}

		protected void OnInitialized()
		{
			_initialized = true;

			if (Initialized != null)
				Initialized(this, new EventArgs());
		}
	}
}

