using Fleck;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Owin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace English
{
    public class ProxyWebServer
    {
        public static string PATH_ROOT = @"./";
        static AutoResetEvent _TERMINAL = new AutoResetEvent(false);

        public static void Start(string baseAddress = "http://+:9000/", string pathRoot = "./")
        {
            PATH_ROOT = pathRoot;

            Task.Factory.StartNew(() =>
            {
                var server = new WebSocketServer("ws://0.0.0.0:8181");
                server.Start(socket =>
                {
                    socket.OnOpen = () =>
                    {
                        //Console.WriteLine("Open!");
                        File.WriteAllText("log.txt", string.Empty);
                    };
                    socket.OnClose = () =>
                    {
                        //Console.WriteLine("Close!");
                    };
                    socket.OnMessage = message =>
                    {
                        //socket.Send(message);
                        Console.WriteLine();
                        Console.WriteLine(message);
                        Console.WriteLine();
                        using (StreamWriter w = File.AppendText("log.txt"))
                        {
                            w.WriteLine(message);
                        }
                    };
                });

                WebApp.Start<Startup>(new StartOptions(baseAddress)
                {
                    ServerFactory = "Microsoft.Owin.Host.HttpListener"
                });
                _TERMINAL.WaitOne();
            });
        }

        public static void Stop()
        {
            _TERMINAL.Set();
        }
    }

    class DisableCacheMiddleWare : OwinMiddleware
    {
        public DisableCacheMiddleWare(OwinMiddleware next) : base(next)
        {
        }
        public override async Task Invoke(IOwinContext context)
        {
            context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";
            await Next.Invoke(context);
        }
    }

    public class CustomHeaderHandler : DelegatingHandler
    {
        //using System.Net.Http; using System.Threading;using System.Threading.Tasks;
        async protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            response.Headers.Add("CACHE-CONTROL", "NO-CACHE");
            return response;
        }
    }

    class Startup
    {
        public AppFunc MyMiddleWare(AppFunc next)
        {
            AppFunc appFunc = async (IDictionary<string, object> context) =>
            {
                var url = context["owin.RequestPath"] as string;
                if (url.Contains("interface.js"))
                {
                    string UserAgent = string.Empty;
                    var requestHeaders = context["owin.RequestHeaders"] as IDictionary<string, string[]>;
                    if (requestHeaders != null && requestHeaders.ContainsKey("User-Agent"))
                        UserAgent = (requestHeaders["User-Agent"] as string[])[0];

                    string root = File.ReadAllText("path.txt");
                    string file = Path.Combine(root, @"home\js\interface\interface.js");
                    if (File.Exists(file))
                    {
                        string js = File.ReadAllText(file);

                        if (UserAgent.Contains("(Custom6"))
                            js = js.Replace("var connectMFP = false;", "var connectMFP = true;");
                        else
                            js = js.Replace("var connectMFP = true;", "var connectMFP = false;");
                        js += File.ReadAllText("proxy.js") + js;

                        //// Do something with the incoming request:
                        var responseHeaders = context["owin.ResponseHeaders"] as IDictionary<string, string[]>;
                        byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(js);
                        responseHeaders.Add("Content-Type", new[] { "application/javascript" });
                        //responseHeaders["Content-Type"] = new string[] { "text/plain" };
                        responseHeaders["Content-Length"] = new string[] { responseBytes.Length.ToString(CultureInfo.InvariantCulture) };

                        var responseStream = context["owin.ResponseBody"] as Stream;
                        await responseStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    }
                }

                // Call the next Middleware in the chain:
                await next.Invoke(context);
            };
            return appFunc;
        }

        public void Configuration(IAppBuilder app)
        {
            //Enable Cors
            app.UseCors(CorsOptions.AllowAll);

            var middleware = new Func<AppFunc, AppFunc>(MyMiddleWare);
            app.Use(middleware);

            //Disable cache
            //app.Use(typeof(DisableCacheMiddleWare));

            // A middleware Disable browser cache for all the request
            //app.Use((ctx, next) =>
            //{
            //    ctx.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            //    ctx.Response.Headers["Pragma"] = "no-cache";
            //    ctx.Response.Headers["Expires"] = "-1"; 
            //    return next();
            //});

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.MessageHandlers.Add(new CustomHeaderHandler());

            app.UseWebApi(config);

            //var physicalFileSystem = new PhysicalFileSystem(@"./www");
            var physicalFileSystem = new PhysicalFileSystem(ProxyWebServer.PATH_ROOT);
            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                RequestPath = new PathString(""),
                EnableDefaultFiles = true,
                FileSystem = physicalFileSystem,
            };

            options.StaticFileOptions.FileSystem = physicalFileSystem;
            options.StaticFileOptions.ServeUnknownFileTypes = true;
            //options.StaticFileOptions.OnPrepareResponse = (StaticFileResponseContext) =>
            //{
            //    StaticFileResponseContext.OwinContext.Response.Headers.Add("Cache-Control", new[] { "public", "max-age=1000" });
            //};
            //options.StaticFileOptions.OnPrepareResponse = context =>
            //{
            //    context.OwinContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            //    context.OwinContext.Response.Headers["Pragma"] = "no-cache";
            //    context.OwinContext.Response.Headers["Expires"] = "0";
            //};
            //options.DefaultFilesOptions.DefaultFileNames = new[]
            //{
            //    "index.html",
            //    "spaHome.html",
            //    "spa.html"
            //}; 
            app.UseFileServer(options);
        }
    }

}
