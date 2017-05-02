using System;
using System.Linq;
using System.Threading.Tasks;
using PagarMe.Mpos.Bridge;
using PagarMe.Mpos.Bridge.Commands;
using PagarMe.Generic;
using PagarMe;
using CommandLine;
using System.Threading;

namespace QuickTest
{
    internal static class Program
    {
        private static Options options;

        private static int Main(string[] args)
        {
            options = new Options()
            {
                BaudRate = Config.BaudRate,
                EncryptionKey = Config.EncryptionKey
            };

            var isValid = Parser.Default.ParseArgumentsStrict(args, options);
            options.EnsureDefaults();

            if (!isValid)
                return 1;

            var bridge = new MposBridge(options);

            var running = true;
            Console.CancelKeyPress += (sender, e) => running = false;

            bridge.Start();

            while (running)
                Thread.Yield();

            bridge.Stop();

            return 0;
        }

        private static async Task ExecuteAsync()
        {
            var bridge = new MposBridge(options);
            bridge.Start();
            var context = bridge.GetContext(null);
            var devices = await context.ListDevices();
            var device = devices.Devices.First(x => x.Name.Contains(Config.DevicePartialName));

            Console.WriteLine(device.Name);

            await context.Initialize(new InitializeRequest
            {
                DeviceId = devices.Devices[1].Id,
                EncryptionKey = Config.EncryptionKey
            });

            var amount = 1;

            var result = await makePayment(context, amount);

            Console.WriteLine(result.Result.CardHash);
        }

        private static async Task<ProcessPaymentResponse> makePayment(Context context, int amount)
        {
            var result = await context.ProcessPayment(new ProcessPaymentRequest
            {
                Amount = amount
            });

            var transaction = new Transaction
            {
                CardHash = result.Result.CardHash,
                Amount = amount,
                ShouldCapture = false
            };

            await transaction.SaveAsync();

            await context.FinishPayment(new FinishPaymentRequest
            {
                Success = true,
                ResponseCode = Int32.Parse(transaction.AcquirerResponseCode),
                EmvData = transaction.CardEmvResponse,
            });

            return result;
        }
    }
}