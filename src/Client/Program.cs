using System;

namespace MySharpChat.Client
{
    static class Program
    {
        static void Main(string[] args)
        {
            AsynchronousClient client = new AsynchronousClient();

            if (client.Start())
            {
                client.Wait();
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }
    }
}
