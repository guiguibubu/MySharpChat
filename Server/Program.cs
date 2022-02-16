using System;

namespace MySharpChat.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            AsynchronousServer.StartListening();
            //SynchronousServer.StartListening();
        }
    }
}
