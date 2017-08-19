using BaristaLabs.ChromeDevTools.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Page = BaristaLabs.ChromeDevTools.Runtime.Page;
using Runtime = BaristaLabs.ChromeDevTools.Runtime.Runtime;


namespace AsyncChromeDriverConsoleApp
{
    class Program
    {

        static void Main(string[] args)
        {
            //Launch Chrome With

            //"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --remote-debugging-port=9223

            Console.WriteLine("Hello World!");

            var sessions = GetSessions("http://localhost:9223/").GetAwaiter().GetResult();

            using (var session = new ChromeSession(sessions.First(s => s.Type == "page").WebSocketDebuggerUrl))
            {
                try
                {
                    //session.Page.SubscribeToLoadEventFiredEvent((e) =>
                    session.Page.SubscribeToDomContentEventFiredEvent((e) =>
                    {
                        Console.WriteLine("Page loaded");
                        ////NOT WORKS
                        //var screenshot = session.Page.CaptureScreenshot(new Page.CaptureScreenshotCommand(), new System.Threading.CancellationToken(), 5000).GetAwaiter().GetResult();
                        //SaveScreenshot(screenshot.Data);

                        // WORKS
                        Task.Run(() =>
                        {
                            var screenshot = session.Page.CaptureScreenshot(new Page.CaptureScreenshotCommand(), new System.Threading.CancellationToken(), 5000).GetAwaiter().GetResult();
                            SaveScreenshot(screenshot.Data);

                        });

                    });
                    session.Page.Enable(new Page.EnableCommand()).GetAwaiter().GetResult();
                    var navigateResult = session.Page.Navigate(new Page.NavigateCommand
                    {
                        Url = "https://www.google.com/"
                    }).GetAwaiter().GetResult();
                    Console.ReadLine();
                    session.Page.Disable(new Page.DisableCommand()).GetAwaiter().GetResult();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        ////C# 7.1
        //static async Task Main(string[] args)
        //{
        //    //Launch Chrome With

        //    //"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --remote-debugging-port=9223
        //    //"C:\Program Files\Opera\47.0.2631.55\opera.exe" --remote-debugging-port=9223

        //    Console.WriteLine("Hello World!");

        //    var sessions = await GetSessions("http://localhost:9223/"); //.GetAwaiter().GetResult();

        //    using (var session = new ChromeSession(sessions.First(s => s.Type == "page").WebSocketDebuggerUrl))
        //    {
        //        try
        //        {
        //            //await session.Page.Enable(new Page.EnableCommand());
        //            ////session.Page.SubscribeToLoadEventFiredEvent(async (e2) =>
        //            //session.Page.SubscribeToDomContentEventFiredEvent(async (e2) =>
        //            //{
        //            //    var screenshot = await session.Page.CaptureScreenshot(new Page.CaptureScreenshotCommand());
        //            //    SaveScreenshot(screenshot.Data);
        //            //});
        //            var navigateResult = await session.Page.Navigate(new Page.NavigateCommand
        //            {
        //                Url = "https://www.bing.com/"
        //            });
        //            Console.ReadLine();
        //            //await session.Page.Disable(new Page.DisableCommand());

        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.ToString());
        //        }
        //    }
        //}

        public static async Task<ICollection<ChromeSessionInfo>> GetSessions(string chromeUrl)
        {
            using (var webClient = new HttpClient())
            {
                webClient.BaseAddress = new Uri(chromeUrl);
                var remoteSessions = await webClient.GetStringAsync("/json");
                return JsonConvert.DeserializeObject<ICollection<ChromeSessionInfo>>(remoteSessions);
            }
        }

        // Works
        //static void Main(string[] args)
        //{
        //    //Launch Chrome With

        //    //"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --remote-debugging-port=9223

        //    Console.WriteLine("Hello World!");

        //    var sessions = GetSessions("http://localhost:9223/").GetAwaiter().GetResult();

        //    using (var session = new ChromeSession(sessions.First(s => s.Type == "page").WebSocketDebuggerUrl))
        //    {

        //        var navigateResult = session.Page.Navigate(new Page.NavigateCommand
        //        {
        //            Url = "https://www.google.com/"
        //        }).GetAwaiter().GetResult();
        //        var screenshot = session.Page.CaptureScreenshot(new Page.CaptureScreenshotCommand()).GetAwaiter().GetResult();
        //        SaveScreenshot(screenshot.Data);
        //        Console.ReadLine();

        //    }
        //}

        private static void SaveScreenshot(string base64String)
        {
            if (!string.IsNullOrWhiteSpace(base64String))
            {
                string path = GetFilePathToSaveScreenshot();
                File.WriteAllBytes(path, Convert.FromBase64String(base64String));
            }
        }

        private static string GetFilePathToSaveScreenshot()
        {
            var dir = @"C:\temp";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var i = 0;
            var path = "";
            do
            {
                i++;
                path = Path.Combine(dir, $"screenshot{i}.png");
            } while (File.Exists(path));
            return path;
        }

    }
}

