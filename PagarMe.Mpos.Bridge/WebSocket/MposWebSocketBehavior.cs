using Newtonsoft.Json;
using NLog;
using PagarMe.Mpos.Bridge.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace PagarMe.Mpos.Bridge.WebSocket
{
    internal class MposWebSocketBehavior : WebSocketBehavior
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private readonly MposBridge mposBridge;

        public MposWebSocketBehavior(MposBridge mposBridge)
        {
            this.mposBridge = mposBridge;
            IgnoreExtensions = true;
        }


        protected override void OnOpen()
        {
            logger.Info("Socket Opened");
        }

        protected override void OnClose(CloseEventArgs e)
        {
            logger.Info($"Socket Closed: [{e.Code}] {e.Reason}");
        }


        protected override async void OnMessage(MessageEventArgs e)
        {
            logger.Info("Request Handling");

            await logger.TryLogOnExceptionAsync(() => 
            {
                return handleMessage(e);
            });
        }

        private async Task handleMessage(MessageEventArgs e)
        {
            var request = JsonConvert.DeserializeObject<PaymentRequest>(e.Data);
            var response = new PaymentResponse();

            var context = mposBridge.GetContext(request.ContextId);

            if (context == null)
            {
                logger.Info("No space for new context");
                response.ResponseType = PaymentResponse.Type.Error;
                response.Error = "Number of transactions opened exceeded, please wait until some of them finishes";
            }
            else
            {
                logger.Info(request.RequestType);
                await handleRequest(context, request, response);
            }

            send(response);
        }

        private async Task handleRequest(Context context, PaymentRequest request, PaymentResponse response)
        {
            var keepProcessing = verifyContextSequence(context, request, response);

            if (!keepProcessing)
                return;

            switch (request.RequestType)
            {
                case PaymentRequest.Type.ListDevices:
                    await getDeviceList(context, response);
                    break;

                case PaymentRequest.Type.Initialize:
                    await initialize(context, request, response);
                    break;

                case PaymentRequest.Type.Process:
                    await process(context, request, response);
                    break;

                case PaymentRequest.Type.Finish:
                    await finish(context, request, response);
                    break;

                case PaymentRequest.Type.DisplayMessage:
                    await displayMessage(context, request, response);
                    break;

                case PaymentRequest.Type.Status:
                    await status(context, response);
                    break;

                case PaymentRequest.Type.Close:
                    await close(context, response);
                    break;

                default:
                    response.ResponseType = PaymentResponse.Type.UnknownCommand;
                    break;
            }
        }

        private Boolean verifyContextSequence(Context context, PaymentRequest request, PaymentResponse response)
        {
            lock (context)
            {
                var canCallAnytime = new[]
                {
                    PaymentRequest.Type.ListDevices,
                    PaymentRequest.Type.DisplayMessage,
                    PaymentRequest.Type.Status
                };

                if (!canCallAnytime.Contains(request.RequestType))
                {
                    var allowed = context.CurrentOperation.GetNextAllowed();

                    if (!allowed.Contains(request.RequestType))
                    {
                        var allowedText = String.Join(", ", allowed);
                        response.Error = $"Just follow operations allowed: {allowedText}";
                        response.ResponseType = PaymentResponse.Type.Error;
                        return false;
                    }

                    context.CurrentOperation = request.RequestType;
                }

                return true;
            }
        }

        private async Task getDeviceList(Context context, PaymentResponse response)
        {
            response.DeviceList = await context.ListDevices();
            response.ResponseType = PaymentResponse.Type.DevicesListed;
        }

        private async Task initialize(Context context, PaymentRequest request, PaymentResponse response)
        {
            await context.Initialize(request.Initialize, onError);

            response.ResponseType = PaymentResponse.Type.Initialized;
        }

        private async Task status(Context context, PaymentResponse response)
        {
            var result = await context.GetStatus();
            response.Status = result;
            response.ResponseType = PaymentResponse.Type.Status;
        }

        private async Task process(Context context, PaymentRequest request, PaymentResponse response)
        {
            var result = await context.ProcessPayment(request.Process);
            response.Process = result.Result;
            response.ResponseType = PaymentResponse.Type.Processed;
        }

        private async Task finish(Context context, PaymentRequest request, PaymentResponse response)
        {
            await context.FinishPayment(request.Finish);
            response.ResponseType = PaymentResponse.Type.Finished;
        }

        private async Task displayMessage(Context context, PaymentRequest request, PaymentResponse response)
        {
            await context.DisplayMessage(request.DisplayMessage);
            response.ResponseType = PaymentResponse.Type.MessageDisplayed;
        }

        private async Task close(Context context, PaymentResponse response)
        {
            await context.Close();
            response.ResponseType = PaymentResponse.Type.Closed;
        }


        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            logger.Error(e.Message);
            logger.Error(e.Exception);
            onError(e.Message);
        }

        private void onError(Int32 errorCode)
        {
            onError($"Error: {errorCode}");
        }

        private void onError(String message)
        {
            var response = new PaymentResponse
            {
                ResponseType = PaymentResponse.Type.Error,
                Error = message
            };

            send(response);
        }

        private void send(PaymentResponse response)
        {
            Send(JsonConvert.SerializeObject(response));
        }

        
    }
}