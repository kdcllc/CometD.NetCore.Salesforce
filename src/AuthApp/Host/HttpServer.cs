using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NetCoreForce.Client;

namespace AuthApp.Host
{
    /// <summary>
    /// Web Server OAuth Authentication Flow
    /// https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/intro_understanding_web_server_oauth_flow.htm
    /// </summary>
    public class HttpServer : BackgroundService
    {
        private readonly SfConfig _config;
        private bool isCompleted = false;

        public HttpServer(SfConfig config)
        {
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"{nameof(HttpServer)} is starting.");
            var http = new HttpListener();
            var redirectURI = string.Format("http://{0}:{1}/", "localhost", GetRandomUnusedPort());
            http.Prefixes.Add(redirectURI);
            http.Start();

            var authUrl = GetAuthorizationUrl(redirectURI);
            Console.WriteLine($"Opening a browser window with Url: {authUrl}");
            ConsoleHandler.HideConsole();

            var process = ConsoleHandler.OpenBrowser(authUrl);
            var context = await http.GetContextAsync();

            while (!stoppingToken.IsCancellationRequested || isCompleted )
            {
                Console.WriteLine($"{nameof(HttpServer)} is running");

                if (context != null)
                {
                    ConsoleHandler.ShowConsole();

                    var responseOutput = await ShowBrowserMessage(context);

                    responseOutput.Close();


                    if (context.Request.QueryString.Get("error") != null)
                    {
                        Console.WriteLine(string.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                    }
                    if (context.Request.QueryString.Get("code") == null)
                    {
                        Console.WriteLine("Malformed authorization response. " + context.Request.QueryString);
                    }

                    // Authorization code the consumer must use to obtain the access and refresh tokens.
                    // The authorization code expires after 15 minutes.
                    var code = context.Request.QueryString.Get("code");

                    var auth = new AuthenticationClient();
                    await auth.WebServerAsync(_config.ClientId,
                       _config.ClientSecret,
                       redirectURI,
                       code,
                       $"{_config.LoginUrl}/services/oauth2/token");
                     


                    Console.WriteLine($"Your access_token is {auth.AccessInfo.AccessToken}");
                    Console.WriteLine($"Your refresh_token is {auth.AccessInfo.RefreshToken}");

                    isCompleted = true;
                }
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            http.Stop();
            Console.WriteLine($"{nameof(HttpServer)} is stopping.");
        }

        private int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 5050);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private string GetAuthorizationUrl(string redirectURI)
        {
            var authEndpoint = $"{_config.LoginUrl}/services/oauth2/authorize";
            var url = $"{authEndpoint}?response_type=code&access_type=offline&scope=openid%20profile%20api%20refresh_token%20offline_access&redirect_uri={Uri.EscapeDataString(redirectURI)}&client_id={_config.ClientId}";
            return url;
        }

        private static async Task<Stream> ShowBrowserMessage(HttpListenerContext context)
        {
            var response = context.Response;
            var responseString = string.Format(@"
                <html>
                    <body>Please return to the console to retrieve access and refresh tokens.</body>
                </html>");

            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            await responseOutput.WriteAsync(buffer, 0, buffer.Length);
            return responseOutput;
        }
    }
}
