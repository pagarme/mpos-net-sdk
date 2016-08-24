using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace PagarMe.Mpos
{
    public class PaymentResult
    {
        public string CardHash { get; private set; }
        public PaymentStatus Status { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }
        public string CardHolderName { get; private set; }

        internal void BuildErrored()
        {
            Status = PaymentStatus.Errored;
        }

		internal async Task BuildAccepted(string encryptionKey, PaymentStatus status, CaptureMethod captureMethod, PaymentMethod method, string pan, string holderName, string expirationDate, string track1, string track2, string track3, string emv, bool isOnlinePin, bool requiredPin, string pin, string pinKek)
        {
			List<Tuple<string, string>> parameters = new List<Tuple<string, string>>();

			parameters.Add(new Tuple<string, string>("capture_method", captureMethod == CaptureMethod.EMV ? "emv" : "magstripe"));
            parameters.Add(new Tuple<string, string>("payment_method", method == PaymentMethod.Credit ? "credit_card" : "debit_card"));
            parameters.Add(new Tuple<string, string>("card_number", pan));
            parameters.Add(new Tuple<string, string>("card_expiration_date", expirationDate));
            
			parameters.Add(new Tuple<string, string>("card_track_2", track2));
			if (track1 != null)
				parameters.Add(new Tuple<string, string>("card_track_1", track1));
			if (track3 != null)
				parameters.Add(new Tuple<string, string>("card_track_3", track3));

			if (holderName != null)
				parameters.Add(new Tuple<string, string>("card_holder_name", holderName));

			if (captureMethod == CaptureMethod.EMV) {
				parameters.Add(new Tuple<string, string> ("card_emv_data", emv));
			}
				
            if (requiredPin)
            {
				parameters.Add(new Tuple<string, string> ("card_pin_mode", isOnlinePin ? "online" : "offline"));

				if (isOnlinePin) {
					parameters.Add (new Tuple<string, string> ("card_pin", pin));
					parameters.Add (new Tuple<string, string> ("card_pin_kek", pinKek));
				}
            }

			parameters.ForEach(Console.WriteLine);
            string urlEncoded = parameters.Select(x => new Tuple<string, string>(x.Item1, Uri.EscapeDataString(x.Item2))).Select(x => x.Item1 + "=" + x.Item2).Aggregate((a, b) => a + "&" + b);

            Status = status;
            PaymentMethod = method;
            CardHolderName = holderName;
            CardHash = await ApiHelper.CreateCardHash(encryptionKey, urlEncoded);
        }
    }
}

