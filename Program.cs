using English;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy102
{
    static class App
    {
        /// <summary>
        /// Helps us to ensure only one instance runs at a time.
        /// </summary>
        static Mutex mutex = new Mutex(true, "{0fbc294c-f089-4009-9b1a-ab757739483f}");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (File.Exists("path.txt") == false) return;

            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                try
                {

                    string PROXY_URL = "http://+:60000/";
                    string PROXY_PATH_ROOT = File.ReadAllText("path.txt");
                    Console.Title = string.Format("{0} -> {1}", PROXY_URL, PROXY_PATH_ROOT);
                    ProxyWebServer.Start(PROXY_URL, PROXY_PATH_ROOT);

                    string cmd = Console.ReadLine();
                    while (cmd != "exit")
                    {
                        switch (cmd)
                        {
                            case "cls":
                            case "clear":
                                File.WriteAllText("log.txt", string.Empty);
                                Console.Clear();
                                break;
                        }
                        cmd = Console.ReadLine();
                    }
                    ProxyWebServer.Stop();
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

    }
}

