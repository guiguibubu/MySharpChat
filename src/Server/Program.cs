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

        Logger logger = Logger.Factory.GetLogger<Program>();

        static int Main(string[] args)
        {
            Server server = new Server(new ConnexionInfos());
            if (server.Start())
            {
                server.Wait();
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

            return server.ExitCode;
        }
    }
}
