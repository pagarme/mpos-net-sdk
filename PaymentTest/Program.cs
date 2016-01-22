using System;
using System.Threading.Tasks;

namespace PaymentTest
{
    class MainClass
    {
        public static void Main(string[] args)
        {

			Process ().Wait ();

            Console.ReadLine();
        }

        public static async Task Process()
        {
			var processor = new PaymentProcessor("/dev/tty.usbmodem14131");

            Console.WriteLine("Welcome to Pagador 9000");
            Console.WriteLine("Initializing...");


            await processor.Initialize();

			//System.Threading.Thread.Sleep (3000);
			Console.Write("Amount: ");
			//String x = Console.ReadLine ();
			//Console.WriteLine ("read line " + x);
			//Int32 integer = Int32.Parse (x);
			await processor.Pay(314);

           // Console.WriteLine("Created transaction {0}.", transaction.Id);
        }
    }
}
