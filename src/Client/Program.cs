using System;

namespace MySharpChat.Client
{
    static class Program
    {
        // TODO Add CLI option for client
        static int Main(string[] args)
        {
            Client client = new Client();

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
