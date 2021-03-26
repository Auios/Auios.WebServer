using Sys = System;
using System.Threading;

using Auios.WebServer.System;

namespace Auios.WebServer
{
    public static class Program
    {
        private static bool runApp = true;
        static void Main(string[] args)
        {
            Console.SetCallbacks(ReturnPressed, EscapePressed);
            while(runApp)
            {
                Console.Update();

                Thread.Sleep(1);
            }
        }

        static void ReturnPressed(string input)
        {

        }

        static void EscapePressed()
        {
            runApp = false;
        }
    }
}
