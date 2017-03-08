using System;
using System.Linq;
using System.Threading.Tasks;
using PagarMe.Mpos.Bridge;
using PagarMe.Mpos.Bridge.Commands;

namespace QuickTest
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                var options = new Options();

                options.EnsureDefaults();

                var bridge = new MposBridge(options);
                var context = bridge.GetContext(null);
                var devices = await context.ListDevices();
                var device = devices.Devices.First(x => x.Name.Contains("PAX"));

                Console.WriteLine(device.Name);

                await context.Initialize(new InitializeRequest
                {
                    DeviceId = devices.Devices[1].Id,
                    EncryptionKey = "ek_test_f9cws0bU9700VqWE4UDuBlKLbvX4IO"
                });

                var result = await context.ProcessPayment(new ProcessPaymentRequest
                {
                    Amount = 1000
                });

                Console.WriteLine(result.Result.CardHash);
            }).Wait();
        }
    }
}