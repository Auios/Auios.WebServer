using Sys = System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Auios.WebServer.System
{
    unsafe public static class Console
    {
        public static string input = string.Empty;
        public static string logFile = "console.log";

        private static bool isRunning = false;
        private static Thread thread;
        private static int historyIndex = 0;
        private static List<string> history = new List<string>() { "" };
        private static Sys.Action<string> returnMethod;
        private static Sys.Action escapeMethod;
        private static Dictionary<string, Sys.Action> commands = new Dictionary<string, Sys.Action>();

        public static void Start()
        {
            isRunning = true;
            thread = new Thread(new ThreadStart(Process));
            thread.Start();
            WriteLine(Sys.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"));
        }

        public static void Stop()
        {
            isRunning = false;
            WriteLine(Sys.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"));
        }

        public static void SetCallbacks(Sys.Action<string> returnMethod = null, Sys.Action escapeMethod = null)
        {
            Console.returnMethod = returnMethod;
            Console.escapeMethod = escapeMethod;
        }

        public static void Write(dynamic input)
        {
            Sys.Console.Write(input);
            File.AppendAllText(logFile, input);
        }

        public static void WriteLine(dynamic input)
        {
            Sys.Console.WriteLine(input);
            File.AppendAllText(logFile, input + Sys.Environment.NewLine);
        }

        public static void Process()
        {
            while(isRunning)
            {
                if(Sys.Console.KeyAvailable)
                {
                    Sys.ConsoleKeyInfo cki = Sys.Console.ReadKey(true);
                    if(cki.KeyChar >= 32 && cki.KeyChar <= 126) // Printable characters
                    {
                        input += cki.KeyChar;
                    }
                    else if(cki.Key == Sys.ConsoleKey.Escape) // Escape
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
                    else if(cki.Key == Sys.ConsoleKey.Enter) // Enter
                    {
                        Sys.Console.SetCursorPosition(0, Sys.Console.CursorTop);

                        if(returnMethod != null) returnMethod.Invoke(input);

                        if(input.Length > 0)
                        {
                            WriteLine($"> '{input}'");

                            if(input != history[^1])
                            {
                                history.Add(input);
                                historyIndex = history.Count;
                            }

                            if(commands.ContainsKey(input))
                            {
                                WriteLine($"[Command: {commands[input].Method.Name}]");
                                commands[input].Invoke();
                            }

                            input = string.Empty;
                        }
                    }

                    Sys.Console.SetCursorPosition(0, Sys.Console.CursorTop);
                    Sys.Console.Write(new string(' ', Sys.Console.WindowWidth));
                    Sys.Console.SetCursorPosition(0, Sys.Console.CursorTop);
                    Sys.Console.Write(input);
                }

                Thread.Sleep(1);
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

        public static void ResetLogFile()
        {
            File.Delete(logFile);
        }

        public static void RegisterCommand(string command, Sys.Action function)
        {
            commands.Add(command, function);
        }
    }
}
