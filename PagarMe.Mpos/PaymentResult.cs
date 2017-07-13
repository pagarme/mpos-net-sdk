using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PagarMe.Mpos.Mpos;

namespace PagarMe.Mpos
{
    public class PaymentResult
    {
        public string CardHash { get; private set; }
        public PaymentStatus Status { get; private set; }
        public Int32? ErrorCode { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }
        public string CardHolderName { get; private set; }
        public bool IsOnlinePin { get; private set; }

        private PaymentStatus status;
        private CaptureMethod captureMethod;
        private PaymentMethod paymentMethod;
        private string pan;
        private string holderName;
        private string expirationDate;
        private int panSequenceNumber;
        private string track1;
        private string track2;
        private string track3;
        private string emv;
        private bool isOnlinePin;
        private bool requiredPin;
        private string pin;
        private string pinKek;

        internal void Fill(Native.PaymentInfo info)
        {
            captureMethod = info.CaptureMethod == Native.CaptureMethod.EMV
                                ? CaptureMethod.EMV
                                : CaptureMethod.Magstripe;
            status = info.Decision == Native.Decision.Refused ? PaymentStatus.Rejected : PaymentStatus.Accepted;
            paymentMethod = (PaymentMethod)info.ApplicationType;
            emv = captureMethod == CaptureMethod.EMV ? GetString(info.EmvData, info.EmvDataLength) : null;
            pan = GetString(info.Pan, info.PanLength);
            expirationDate = GetString(info.ExpirationDate);
            holderName = info.HolderNameLength.ToInt32() > 0
                ? GetString(info.HolderName, info.HolderNameLength)
                : null;
            panSequenceNumber = info.PanSequenceNumber;
            pin = null;
            pinKek = null;
            isOnlinePin = info.IsOnlinePin != 0;
            requiredPin = info.PinRequired != 0;

            track1 = info.Track1Length.ToInt32() > 0 ? GetString(info.Track1, info.Track1Length) : null;
            track2 = GetString(info.Track2, info.Track2Length);
            track3 = info.Track3Length.ToInt32() > 0 ? GetString(info.Track3, info.Track3Length) : null;

            expirationDate = expirationDate.Substring(2, 2) + expirationDate.Substring(0, 2);
            if (holderName != null)
                holderName = holderName.Trim().Split('/').Reverse().Aggregate((a, b) => a + ' ' + b);

            if (requiredPin && isOnlinePin)
            {
                pin = GetString(info.Pin);
                pinKek = GetString(info.PinKek);
            }
        }

        internal void BuildErrored(Int32 error)
        {
            Status = error == Native.ST_CANCEL 
                ? PaymentStatus.Canceled 
                : PaymentStatus.Errored;

            ErrorCode = error;
        }

        internal async Task BuildAccepted(string encryptionKey)
        {
            var parameters = new List<Tuple<string, string>>();

            parameters.Add(new Tuple<string, string>("capture_method",
                captureMethod == CaptureMethod.EMV ? "emv" : "magstripe"));
            parameters.Add(new Tuple<string, string>("payment_method",
                paymentMethod == PaymentMethod.Credit ? "credit_card" : "debit_card"));
            parameters.Add(new Tuple<string, string>("card_number", pan));
            parameters.Add(new Tuple<string, string>("card_expiration_date", expirationDate));
            parameters.Add(new Tuple<string, string>("card_sequence_number", panSequenceNumber.ToString()));

            parameters.Add(new Tuple<string, string>("card_track_2", track2));
            if (track1 != null)
                parameters.Add(new Tuple<string, string>("card_track_1", track1));
            if (track3 != null)
                parameters.Add(new Tuple<string, string>("card_track_3", track3));

            if (holderName != null)
                parameters.Add(new Tuple<string, string>("card_holder_name", holderName));

            if (captureMethod == CaptureMethod.EMV) parameters.Add(new Tuple<string, string>("card_emv_data", emv));

            if (requiredPin)
            {
                parameters.Add(new Tuple<string, string>("card_pin_mode", isOnlinePin ? "online" : "offline"));

                if (isOnlinePin)
                {
                    parameters.Add(new Tuple<string, string>("card_pin", pin));
                    parameters.Add(new Tuple<string, string>("card_pin_kek", pinKek));
                }
            }

            parameters.ForEach(Console.WriteLine);
            var urlEncoded =
                parameters.Select(x => new Tuple<string, string>(x.Item1, Uri.EscapeDataString(x.Item2)))
                    .Select(x => x.Item1 + "=" + x.Item2)
                    .Aggregate((a, b) => a + "&" + b);

            Status = status;
            PaymentMethod = paymentMethod;
            CardHolderName = holderName;
            CardHash = await ApiHelper.CreateCardHash(encryptionKey, urlEncoded);
            IsOnlinePin = isOnlinePin;
        }
    }
}