using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SubtitleDubber.Utils
{
    internal class CommandExecutor
    {
        public static void Execute(string executable, string parameters, string workingDirectory = null)
        {

            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = parameters;
if (!string.IsNullOrEmpty(workingDirectory))
                {
                    p.StartInfo.WorkingDirectory = workingDirectory;
                                    }
                p.Start();
                p.WaitForExit();
            }
        }
    }
}
