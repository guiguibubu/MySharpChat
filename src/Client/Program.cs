using System;

namespace MySharpChat.Client
{
    static class Program
    {
        static int Main(string[] args)
        {
            AsynchronousClient client = new AsynchronousClient();

            if (client.Start())
            {
                client.Wait();
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

            return client.ExitCode;
        }
    }
}
