using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Mem2G
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async ()=> {
                while (true)
                {
                    Thread.Sleep(10000);
                    await Look();
                }
            });

            while (true)
            {
                Thread.Sleep(99999);
            }
        }

        static async Task Look()
        {
            Process? p = null;
            try
            {
                p = new Process();
                p.StartInfo = new ProcessStartInfo("cat", "/proc/meminfo");
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                try
                {
                    while (true)
                    {
                        var line = await p.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(2));
                        if (line == null)
                        {
                            break;
                        }
                        var match = Regex.Match(line, @"MemAvailable:\s*(\d+)");
                        if (match.Success)
                        {
                            int a = int.Parse(match.Groups[1].ValueSpan);
                            if (a <= 40000)
                                Process.Start("killall", "-9 poscan-consensus");
                            break;
                        }
                    }
                }
                catch (TimeoutException)
                {
                    Process.Start("killall", "-9 poscan-consensus");
                }
            }
            finally
            {
                p?.Kill();
            }
        }

    }
}