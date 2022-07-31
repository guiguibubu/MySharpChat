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

        private static readonly Logger logger = Logger.Factory.GetLogger<Program>();

        // TODO Add CLI option for client
        private static int Main(string[] args)
        {
            try
            {
                Client client = new Client(new ConsoleClientImpl());

                if (client.Start())
                {
                    client.Wait();
                }

                System.Console.WriteLine("\nPress ENTER to continue...");
                System.Console.Read();

                return client.ExitCode;
            }
            catch(Exception e)
            {
                logger.LogCritical(e, "Program crash !");
                System.Console.WriteLine("Program crash ! : {0}", e);
                return 1;
            }
        }
    }
}
