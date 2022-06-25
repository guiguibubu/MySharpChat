using System;

namespace MySharpChat.Server
{
    static class Program
    {
        static void Main(string[] args)
        {
            AsynchronousServer.StartListening();
        }
    }
}
