using Newtonsoft.Json;
using PagarMe.Mpos.Bridge.Commands;
using System;
using System.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace PagarMe.Mpos.Bridge.WebSocket
{
    internal class MposWebSocketBehavior : WebSocketBehavior
    {
        private MposBridge mposBridge;
        private Context context;

        public MposWebSocketBehavior(MposBridge mposBridge)
        {
            this.mposBridge = mposBridge;

            var requestCode = GetHashCode().ToString();
            context = mposBridge.GetContext(requestCode);
        }

        protected override async void OnMessage(MessageEventArgs e)
        {
            var request = JsonConvert.DeserializeObject<PaymentRequest>(e.Data);
            var response = new PaymentResponse();

            switch (request.RequestType)
            {
                case PaymentRequest.Type.Process:
                    var result = await context.ProcessPayment(request.Process);
                    response.CardHash = result.Result.CardHash;
                    response.ResponseType = PaymentResponse.Type.Processed;
                    break;

                case PaymentRequest.Type.Finish:
                    await context.FinishPayment(request.Finish);
                    response.ResponseType = PaymentResponse.Type.Finished;
                    break;

                default:
                    response.ResponseType = PaymentResponse.Type.UnknownCommand;
                    break;
            }

            Send(JsonConvert.SerializeObject(response));

            base.OnMessage(e);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            var response = new PaymentResponse {
                ResponseType = PaymentResponse.Type.Error,
                Error = e.Message
            };
            Send(JsonConvert.SerializeObject(response));

            base.OnError(e);
        }

        protected override async void OnOpen()
        {
            var devices = mposBridge.DeviceManager.FindAvailableDevices();
            var device = devices.Last();

            await context.Initialize(new InitializeRequest
            {
                DeviceId = device.Id,
                EncryptionKey = mposBridge.Options.EncryptionKey
            });

            base.OnOpen();
        }

        protected override async void OnClose(CloseEventArgs e)
        {
            await context.Close();
            context.Dispose();
            base.OnClose(e);
        }






    }
}