using Sys = System;
using System.Net;

using NLua;

using Auios.WebServer.System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Auios.WebServer
{
    public delegate void ReceiveWebRequest(HttpListenerContext Context);

    public class HttpServer
    {
        private HttpListener listener;
        private string publicPath;
        private Dictionary<string, Route> routes;
        private event ReceiveWebRequest receiveWebRequest;

        private Lua lua;

        public HttpServer(string viewsPath)
        {
            publicPath = viewsPath;
            listener = new HttpListener();
            routes = new Dictionary<string, Route>();

            routes.Add("/", new Route("/", "Home", "/views/index.html"));
            routes.Add("/404", routes["/"]);

            lua = new Lua();
        }

        public void Start()
        {
            Console.WriteLine("Starting HttpServer...");

            if(!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            if(listener.Prefixes.Count == 0)
            {
                throw new Sys.Exception("Listener has no prefixes");
            }
            listener.Start();
            Sys.IAsyncResult result = listener.BeginGetContext(new Sys.AsyncCallback(RequestCallback), listener);
        }

        public void Stop()
        {
            Console.WriteLine("Stopping HttpServer...");
            listener.Stop();
        }

        public void AddPrefix(string prefix)
        {
            listener.Prefixes.Add(prefix);
        }

        public void AddRoute(Route route)
        {
            if(routes.ContainsKey(route.name))
            {
                routes[route.name] = route;
            }
            else
            {
                routes.Add(route.name, route);
            }
        }

        private string ProcessPage(string fileName)
        {
            string contents = File.ReadAllText(fileName);
            string html = string.Empty;
            string luaCmd = string.Empty;

            bool isLua = false;

            for(int i = 0; i < contents.Length; i += 1)
            {
                if(isLua)
                {
                    luaCmd += contents[i];
                }
                else
                {
                    html += contents[i];
                }
            }

            return html;
        }

        private void RequestCallback(Sys.IAsyncResult result)
        {
            HttpListenerContext context = null;
            try
            {
                context = listener.EndGetContext(result);
                listener.BeginGetContext(new Sys.AsyncCallback(RequestCallback), listener);
            }
            catch(Sys.Exception)
            {
                return;
            }

            receiveWebRequest?.Invoke(context);

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{request.HttpMethod} {request.RawUrl} Http/{request.ProtocolVersion}");
            if(request.UrlReferrer != null) sb.AppendLine($"Referer: {request.UrlReferrer}");
            if(request.UserAgent != null) sb.AppendLine($"User-Agent: {request.UserAgent}");

            for(int i = 0; i < request.Headers.Count; i += 1)
            {
                sb.AppendLine($"{request.Headers.Keys[i]}: {request.Headers[i]}");
            }

            sb.AppendLine();

            IPEndPoint ip = request.RemoteEndPoint;
            Sys.Uri url = request.Url;
            string path = url.AbsolutePath;

            byte[] buffer;
            if(path == "/favicon.ico")
            {
                buffer = File.ReadAllBytes(publicPath + "/favicon.ico");
                response.ContentType = "image/x-icon";
            }
            else
            {
                if(!routes.ContainsKey(path))
                {
                    path = routes["/404"].name;
                    response.StatusCode = 404;
                }
                string html = ProcessPage(publicPath + routes[path].path);
                buffer = Encoding.UTF8.GetBytes(html);
                response.ContentType = "text/html";
            }

            Console.WriteLine($"{Sys.DateTime.Now}\t{ip}\t{response.StatusCode}\t{path}");

            if(buffer != null)
            {
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
        }
    }
}
