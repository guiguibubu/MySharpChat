using MySharpChat.Core.Utils.Logger;
using System;
using System.Diagnostics;

namespace MySharpChat.Client
{
    class Program
    {
        static Program()
        {
            Logger.Factory.SetLoggingType(LoggerType.File);
        }

        protected Program()
        {

        }

        Logger logger = Logger.Factory.GetLogger<Program>();

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
