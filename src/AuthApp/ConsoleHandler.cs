using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AuthApp
{
    public class ConsoleHandler
    {

        public static void ShowConsole()
        {
            var console = GetConsoleWindow();
            ShowWindow(console, 5);
        }

        public static void HideConsole()
        {
            var console = GetConsoleWindow();
            ShowWindow(console, 0);
        }

#pragma warning disable IDE0040 // Add accessibility modifiers

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

#pragma warning restore IDE0040 // Add accessibility modifiers


        public static Process OpenBrowser(string url)
        {
            try
            {
                return Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    return Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
