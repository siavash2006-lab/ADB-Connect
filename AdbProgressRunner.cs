using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ADB_Connect
{
    public class AdbProgressRunner
    {
        public async Task RunAdbWithProgressAsync(
            string arguments,
            Action<int> onProgress,
            int timeoutMs = 300000)
        {
            var psi = new ProcessStartInfo
            {
                FileName = AdbRunner.AdbPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var p = new Process { StartInfo = psi };

            p.OutputDataReceived += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(e.Data))
                    return;

                // نمونه خروجی: [ 56%]
                var match = Regex.Match(e.Data, @"\[\s*(\d+)%\]");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int percent))
                {
                    onProgress(percent);
                }
            };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            await Task.Run(() => p.WaitForExit(timeoutMs));
        }
    }
}
