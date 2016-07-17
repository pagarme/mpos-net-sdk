using System;

namespace PagarMe.Mpos
{
	public struct EmvApplication {
		public string Brand;
		public PaymentMethod PaymentMethod;

		public EmvApplication(string brand, PaymentMethod paymentMethod) {
			Brand = brand;
			PaymentMethod = paymentMethod;
		}
	}
}

