using System;
using System.Linq;
using MySharpChat.Core.Utils.Logger;

namespace MySharpChat.Server.Console
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
            using var db = new BloggingContext();

            // Note: This sample requires the database to be created before running.
            System.Console.WriteLine($"Database path: {db.DbPath}.");

            // Create
            System.Console.WriteLine("Inserting a new blog");
            db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
            db.SaveChanges();

            // Read
            System.Console.WriteLine("Querying for a blog");
            var blog = db.Blogs
                .OrderBy(b => b.BlogId)
                .First();

            // Update
            System.Console.WriteLine("Updating the blog and adding a post");
            blog.Url = "https://devblogs.microsoft.com/dotnet";
            blog.Posts.Add(
                new Post { Title = "Hello World", Content = "I wrote an app using EF Core!" });
            db.SaveChanges();

            // Delete
            System.Console.WriteLine("Delete the blog");
            db.Remove(blog);
            db.SaveChanges();







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
                System.Console.WriteLine("Program crash ! : {0}", e);
                exitCode = 1;
            }
            System.Console.WriteLine("\nPress ENTER to continue...");
            System.Console.Read();
            return exitCode;
        }
    }
}
