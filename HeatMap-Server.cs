using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace HeatMapServer {
    class HttpServer {
        public static HttpListener listener;
        public static string url = "http://localhost:6969/heatmap-server/";
        public static int pageViews = 0;
        public static int requestCount = 0;

        public static string exampleSVG = File.ReadAllText("example.svg", Encoding.UTF8);

        public static async Task HandleIncomingConnections() {

            while (true) {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                if (!req.Url.ToString().EndsWith("ws")) {
                    Console.WriteLine("Request #: {0}", ++requestCount);
                    Console.WriteLine(req.Url.ToString());
                    Console.WriteLine(req.HttpMethod);
                    Console.WriteLine(req.UserHostName);
                    Console.WriteLine(req.UserAgent);
                    Console.WriteLine();
                }

                if (req.Url.ToString() == url + "example.svg") {
                    // Write the response info
                    byte[] data = Encoding.UTF8.GetBytes(exampleSVG);
                    resp.ContentType = "image/svg+xml";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;

                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                } else {
                    resp.StatusCode = 404;
                    resp.StatusDescription = "NOT FOUND";
                    resp.Close();
                }                
            }
        }


        public static void Main(string[] args) {
            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }
    }
}