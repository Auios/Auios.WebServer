using Sys = System;
using System.Collections.Generic;
using System.IO;

namespace Auios.WebServer.System
{
    unsafe public static class Console
    {
        public static string input = string.Empty;
        public static string outputFile = "console.log";

        private static int historyIndex = 0;
        private static List<string> history = new List<string>() { "" };
        private static Sys.Action<string> returnMethod;
        private static Sys.Action escapeMethod;

        public static void SetCallbacks(Sys.Action<string> returnMethod = null, Sys.Action escapeMethod = null)
        {
            Console.returnMethod = returnMethod;
            Console.escapeMethod = escapeMethod;
        }

        public static void Write(dynamic input)
        {
            Sys.Console.Write(input);
            File.AppendAllText(outputFile, input);
        }

        public static void WriteLine(dynamic input)
        {
            Sys.Console.WriteLine(input);
            File.AppendAllText(outputFile, input + Sys.Environment.NewLine);
        }

        public static void Update()
        {
            if(Sys.Console.KeyAvailable)
            {
                Sys.ConsoleKeyInfo cki = Sys.Console.ReadKey(false);
                if(cki.KeyChar >= 32 && cki.KeyChar <= 126) // Printable characters
                {
                    input += cki.KeyChar;
                }
                else if(cki.KeyChar == 27) // Escape
                {
                    if(escapeMethod != null) escapeMethod.Invoke();
                }
                else if(cki.KeyChar == 8 && input.Length > 0) // Backspace
                {
                    input = input.Remove(input.Length - 1, 1);
                }
                else if(cki.Key == Sys.ConsoleKey.UpArrow) // Up
                {
                    if(historyIndex > 0)
                    {
                        historyIndex--;
                        input = history[historyIndex];
                    }
                    else
                    {
                        input = string.Empty;
                    }
                }
                else if(cki.Key == Sys.ConsoleKey.DownArrow) // Down
                {
                    if(historyIndex < history.Count - 1)
                    {
                        historyIndex++;
                        input = history[historyIndex];
                    }
                    else
                    {
                        historyIndex = history.Count;
                        input = string.Empty;
                    }
                }
                else if(cki.KeyChar == 13) // Enter
                {
                    Sys.Console.SetCursorPosition(0, Sys.Console.CursorTop);
                    WriteLine(input);
                    if(input.Length > 0)
                    {
                        WriteLine($"% '{input}'");

                        if(input != history[^1])
                        {
                            history.Add(input);
                            historyIndex = history.Count;
                        }

                        if(returnMethod != null) returnMethod.Invoke(input);
                        input = string.Empty;
                    }
                }

                Sys.Console.SetCursorPosition(0, Sys.Console.CursorTop);
                Sys.Console.Write(new string(' ', Sys.Console.WindowWidth));
                Sys.Console.SetCursorPosition(0, Sys.Console.CursorTop);
                Sys.Console.Write(input);
            }
        }

        public static void SetColor(Sys.ConsoleColor color)
        {
            Sys.Console.ForegroundColor = color;
        }

        public static void ResetColor()
        {
            Sys.Console.ForegroundColor = Sys.ConsoleColor.Gray;
        }
    }
}
