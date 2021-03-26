namespace Auios.WebServer.System
{
    public class Route
    {
        public string name;
        public string title;
        public string path;

        public Route(string name, string title, string path)
        {
            this.name = name;
            this.title = title;
            this.path = path;
        }
    }
}
