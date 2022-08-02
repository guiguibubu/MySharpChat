using MySharpChat.Core.Packet;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MySharpChat.Client.Console
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
            int exitCode;
            try
            {
                System.Console.Clear();

                Client client = new Client(new ConsoleClientImpl());

                if (client.Start())
                {
                    client.Wait();
                    logger.LogInfo("Client stopped !");
                    System.Console.WriteLine("Client stopped !");
                }

                exitCode = client.ExitCode;
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Program crash !");
                System.Console.WriteLine("Program crash ! : {0}", e);
                exitCode = 1;
            }

            System.Console.WriteLine("\nPress ENTER to continue...");
            System.Console.Read();

            System.Console.Clear();

            return exitCode;
        }
    }
}
