using System;

namespace PaymentTest
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            PaymentProcessor processor = new PaymentProcessor();

            processor.Pay(250).Wait();

            Console.ReadLine();
        }
    }
}
