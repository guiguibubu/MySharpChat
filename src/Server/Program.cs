using MySharpChat.Core.Utils.Logger;
using System;

namespace MySharpChat.Server
{
    class Program
    {
        static Program()
        {
            Logger.Factory.SetLoggingType(LoggerType.Both);
        }

        protected Program()
        {

        }

        private static readonly Logger logger = Logger.Factory.GetLogger<Program>();

        private static int Main(string[] args)
        {
            int exitCode;
            try
            {
                Server server = new Server(new ConsoleServerImpl());
                if (server.Start())
                {
                    server.Wait();
                }

                exitCode = server.ExitCode;
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Program crash !");
                Console.WriteLine("Program crash ! : {0}", e);
                exitCode = 1;
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
            return exitCode;
        }
    }
}
