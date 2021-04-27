namespace Tests
{
    class Test_Console
    {
        public void Test_WriteLine()
        {
            string x = Auios.WebServer.System.Console.input;
            Program.Assert(x == string.Empty);
        }
    }
}
