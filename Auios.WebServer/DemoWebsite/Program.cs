using System.Threading;

using Auios.WebServer;
using Auios.WebServer.System;

using DemoWebsite.ConsoleCommands;

namespace DemoWebsite
{
    public static class Program
    {
        private static HttpServer httpServer;

        static void Main()
        {
            Console.ResetLogFile();
            Console.RegisterCommand("stop", Stop);
            Console.RegisterCommand("cls", Commands.Clear);
            Console.Start();

            httpServer = new HttpServer("public/");
            httpServer.AddPrefix("http://localhost:8080/");

            // Setup routes
            SetupRoutes();

            // Start server
            httpServer.Start();
        }

        static private void Stop()
        {
            Console.WriteLine("Stopping...");
            httpServer.Stop();
            Console.Stop();
        }

        static private void SetupRoutes()
        {
            Console.WriteLine("Adding routes...");

            httpServer.AddRoute(new Route("/", "Home", "/views/index.html"));
            httpServer.AddRoute(new Route("/404", "404", "/views/404.html"));
        }
    }
}
