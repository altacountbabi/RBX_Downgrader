using System.Diagnostics;
using System.IO;

namespace RBX_Downgrader
{
    public class PSRunner
    {
        public delegate void stdout_callback_delegate(string data);

        public void execute(string script, stdout_callback_delegate stdout_callback = null)
        {
            string sanitizedScript = script.Replace("\"", "'").Replace("\n", "\\n");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{sanitizedScript}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        if (stdout_callback == null) return;
                        stdout_callback(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            }
        }
    }
}
