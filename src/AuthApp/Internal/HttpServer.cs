using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using NetCoreForce.Client;

using TextCopy;

using Console = Colorful.Console;

namespace AuthApp.Internal
{
    /// <summary>
    /// Web Server OAuth Authentication Flow
    /// https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/intro_understanding_web_server_oauth_flow.htm.
    /// </summary>
    internal class HttpServer : BackgroundService
    {
        private readonly HostBuilderOptions _hostOptions;
        private readonly SfConfig _config;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private bool _isCompleted = false;

        public HttpServer(
            HostBuilderOptions hostOptions,
            SfConfig config,
            IHostApplicationLifetime applicationLifetime)
        {
            _hostOptions = hostOptions;
            _config = config;
            _applicationLifetime = applicationLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine();
            Console.WriteAscii("Token Generation Started...", Colorful.FigletFont.Default);
            Console.WriteLine();

            if (_hostOptions.Verbose)
            {
                Console.WriteLine($"{nameof(HttpServer)} is starting.");
            }

            var http = new HttpListener();
            var redirectURI = string.Format("http://{0}:{1}/", "localhost", GetRandomUnusedPort());
            http.Prefixes.Add(redirectURI);
            http.Start();

            var authUrl = GetAuthorizationUrl(redirectURI);

            if (_hostOptions.Verbose)
            {
                Console.WriteLine($"Opening a browser window with Url: {authUrl}", Color.Blue);
            }

            var process = ConsoleHandler.OpenBrowser(authUrl);

            var context = await http.GetContextAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_isCompleted)
                {
                    _applicationLifetime.StopApplication();
                    return;
                }

                if (_hostOptions.Verbose)
                {
                    Console.WriteLine($"{nameof(HttpServer)} is running");
                }

                if (context != null)
                {
                    var responseOutput = await ShowBrowserMessage(context);

                    responseOutput.Close();

                    if (context.Request.QueryString.Get("error") != null)
                    {
                        Console.WriteLine($"OAuth authorization error: {context.Request.QueryString.Get("error")}.", Color.Red);
                    }

                    if (context.Request.QueryString.Get("code") == null)
                    {
                        Console.WriteLine($"Malformed authorization response {context.Request.QueryString}", Color.Red);
                    }

                    // Authorization code the consumer must use to obtain the access and refresh tokens.
                    // The authorization code expires after 15 minutes.
                    var code = context.Request.QueryString.Get("code");
                    Console.WriteLine($"The authorization code will expire in 15 minutes: {code}", Color.Blue);

                    var auth = new AuthenticationClient();
                    await auth.WebServerAsync(
                        _config.ClientId,
                        _config.ClientSecret,
                        redirectURI,
                        code,
                        $"{_config.LoginUrl}{_config.OAuthUri}");

                    Console.WriteLineFormatted("Access_token = {0}", Color.Green, Color.Yellow, auth.AccessInfo.AccessToken);

                    Console.WriteLineFormatted("Refresh_token = {0}", Color.Green, Color.Yellow, auth.AccessInfo.RefreshToken);

                    Clipboard.SetText(auth.AccessInfo.RefreshToken);

                    Console.WriteLine($"Refresh_token copied to the Clipboard", color: Color.Yellow);

                    _isCompleted = true;

                    http.Stop();
                    if (_hostOptions.Verbose)
                    {
                        Console.WriteLine($"{nameof(HttpServer)} is stopping.");
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                Console.WriteLine();
                Console.Write("Thanks for using this cli tool");
            }
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
            var authEndpoint = $"{_config.LoginUrl}{_config.OAuthorizeUri}";
            var url = $"{authEndpoint}?response_type=code&access_type=offline&scope=openid%20profile%20api%20refresh_token%20offline_access&redirect_uri={Uri.EscapeDataString(redirectURI)}&client_id={_config.ClientId}";
            return url;
        }
    }
}
