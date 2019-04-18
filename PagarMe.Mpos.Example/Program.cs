using System;
using System.Threading.Tasks;

namespace PagarMe.Mpos.Example
{
    internal class MainClass
    {
        public static void Main(string[] args)
        {
            Process().Wait();

            Console.ReadLine();
        }

        public static async Task Process()
        {
            var processor = new PaymentProcessor(Config.Port);

            PgDebugLog.Write("Will initialize");
            await processor.Initialize();
            PgDebugLog.Write("Initialized");

            await processor.Pay(100);
        }
    }
}
