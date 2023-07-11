using System.Net;
using Spectre.Console;

namespace RBX_Downgrader
{
    class Downloader
    {
        public void download(string name, string url, string save_path)
        {
            AnsiConsole.Status()
                .Start($"Downloading {name}..", ctx =>
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(url, save_path);
                    }
                });
        }
    }
}
