using System;

namespace MySharpChat.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            AsynchronousClient.StartClient();
            //SynchronousClient.StartClient();
        }
    }
}
