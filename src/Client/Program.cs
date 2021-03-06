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

        readonly Logger logger = Logger.Factory.GetLogger<Program>();

        // TODO Add CLI option for client
        static int Main(string[] args)
        {
            Client client = Client.Factory.Initialize();

            if (client.Start())
            {
                client.Wait();
            }

            System.Console.WriteLine("\nPress ENTER to continue...");
            System.Console.Read();

            return client.ExitCode;
        }
    }
}
