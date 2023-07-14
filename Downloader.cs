using System.IO;
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
                    WebRequest request = WebRequest.Create(url);
                    request.Method = "GET";
                    request.Timeout = 99999999;

                    using (WebResponse response = request.GetResponse())
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            using (FileStream fileStream = File.Create(save_path))
                            {
                                byte[] buffer = new byte[1024];
                                int bytesRead;

                                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fileStream.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                });
        }
    }
}
