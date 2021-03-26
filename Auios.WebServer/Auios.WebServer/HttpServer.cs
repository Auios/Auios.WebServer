using Sys = System;
using System.Net;

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

        public HttpServer(string viewsPath)
        {
            publicPath = viewsPath;
            listener = new HttpListener();
            routes = new Dictionary<string, Route>();

            routes.Add("/", new Route("/", "Home", "/views/index.html"));
            routes.Add("/404", routes["/"]);
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

            Console.WriteLine($"{Sys.DateTime.Now}\t{ip}\t{path}");

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
                }
                string responseBody = File.ReadAllText(publicPath + routes[path].path);
                buffer = Encoding.UTF8.GetBytes(responseBody);
                response.ContentType = "text/html";
            }

            if(buffer != null)
            {
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
        }
    }
}
