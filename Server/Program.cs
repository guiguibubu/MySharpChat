using System;

namespace Serveur
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
