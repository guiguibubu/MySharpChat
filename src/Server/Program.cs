using System;

namespace MySharpChat.Server
{
    static class Program
    {
        static void Main(string[] args)
        {
            AsynchronousServer server = new AsynchronousServer(new ConnexionInfos());
            if (server.Start())
            {
                server.Wait();
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }
    }
}
