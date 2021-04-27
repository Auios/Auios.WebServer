using Sys = System;
using System.Collections.Generic;
using System.Reflection;

namespace Tests
{
    public static class Program
    {
        #region System
        // ===== System =====

        private static List<string> passedTests = new List<string>();
        private static List<string> failedTests = new List<string>();
        private static List<Sys.Type> types = new List<Sys.Type>();

        private static string currentTestName;
        private static bool currentTestFailed;

        private static void GetTypes()
        {
            Sys.Type[] types = Assembly.GetCallingAssembly().GetTypes();
            foreach(var type in types)
            {
                if(type.Name.StartsWith("Test_"))
                {
                    Program.types.Add(type);
                }
            }
        }

        private static void RunTests()
        {
            foreach(Sys.Type type in types)
            {
                MethodInfo[] allMethods = type.GetMethods();
                foreach(MethodInfo method in allMethods)
                {
                    if(method.Name.StartsWith("Test_"))
                    {
                        object instance = null;

                        if(!IsStaticClass(type))
                            instance = Sys.Activator.CreateInstance(type);

                        BeginTest(method.Name);
                        {
                            method.Invoke(instance, null);
                        }
                        EndTest();
                    }
                }
            }
        }

        public static void Assert(bool shouldBeTrue)
        {
            if(!shouldBeTrue) currentTestFailed = true;
        }

        private static void BeginTest(string testName)
        {
            if(currentTestName != null)
            {
                TestWasNotProperlyEnded();
            }
            else
            {
                currentTestFailed = false;
                currentTestName = testName;
            }
        }

        private static void EndTest()
        {
            if(currentTestFailed)
            {
                Sys.Console.ForegroundColor = Sys.ConsoleColor.Red;
                failedTests.Add(currentTestName);
            }
            else
            {
                Sys.Console.ForegroundColor = Sys.ConsoleColor.Green;
                passedTests.Add(currentTestName);
            }

            Sys.Console.WriteLine(currentTestName);
            currentTestName = null;
            Sys.Console.ResetColor();
        }

        private static bool IsStaticClass(Sys.Type type)
        {
            return type.GetConstructor(Sys.Type.EmptyTypes) == null && type.IsAbstract && type.IsSealed;
        }

        private static void TestWasNotProperlyEnded()
        {
            Sys.Console.ForegroundColor = Sys.ConsoleColor.Red;
            Sys.Console.WriteLine($"Test \"{currentTestName}\" was not properly ended!");
            Sys.Console.ResetColor();
            Sys.Console.ReadKey(true);
            Sys.Environment.Exit(0);
        }

        private static void CompileResults()
        {
            if(currentTestName != null)
            {
                TestWasNotProperlyEnded();
            }
            else
            {
                Sys.Console.ForegroundColor = Sys.ConsoleColor.Cyan;
                Sys.Console.WriteLine("\n=== Results ===");

                Sys.Console.ForegroundColor = Sys.ConsoleColor.Green;
                Sys.Console.WriteLine($"Passed: {passedTests.Count}");

                Sys.Console.ForegroundColor = Sys.ConsoleColor.Red;
                Sys.Console.WriteLine($"Failed: {failedTests.Count}");

                Sys.Console.ResetColor();
            }
            Sys.Console.ReadKey(true);
        }

        private static void Main()
        {
            GetTypes();
            RunTests();
            CompileResults();
        }
        #endregion
    }
}
