using System;
using System.Threading.Tasks;

namespace PagarMe.Mpos.Example
{
    internal class MainClass
    {
        public static void Main(string[] args)
        {
            try
            {
                Process().Wait();
            }
            catch (Exception e)
            {
                PgDebugLog.Write(e);
            }

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
