using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SubtitleDubber.Utils.Commands;

namespace SubtitleDubber.Utils
{
    internal class CommandExecutor
    {
                public static string Execute(Command command)
        {
            command.InitializeArguments();
            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = command.Executable;
                p.StartInfo.Arguments = ConvertToArgumentString(command.Arguments);
if (!string.IsNullOrEmpty(command.WorkingDirectory))
                {
                    p.StartInfo.WorkingDirectory = command.WorkingDirectory;
                                    }
                p.StartInfo.RedirectStandardOutput = true;

                p.Start();
                p.WaitForExit();
                var output = p.StandardOutput.ReadToEnd();
                return output;
            }
        }

        private static string ConvertToArgumentString(List<String> arguments)
        {
            return string.Join(" ", arguments);
        }

        public static string ExecuteSilenceCommand(string inputFileName, string outputFileName, long msAtStart, long msAtEnd)
        {
            var command = new SilenceCommand();
            command.InputFileName = inputFileName;
            command.OutputFileName = outputFileName;
            command.MsAtStart = msAtStart;
            command.MsAtEnd = msAtEnd;
            return Execute(command);
        }

        public static string ExecuteSubtitleListCommand(string inputFileName)
        {
            var command = new SubtitleListCommand();
            command.InputFileName = inputFileName;
            return Execute(command);
        }

        public static string ExecuteDownloadSubtitleCommand(string inputFileName, string outputFileName, string subtitleFormat, int subtitleTrackId)
        {
            var command = new DownloadSubtitleCommand();
            command.InputFileName = inputFileName;
            command.OutputFileName = outputFileName;
            command.SubtitleFormat = subtitleFormat;
            command.SubtitleTrackId = subtitleTrackId;
            return Execute(command);
        }

        public static string ExecuteConcatFilesCommand(List<string> parameters, string directory)
        {
            var concatCommand = new SOXCommand();
            concatCommand.Arguments = parameters;
            concatCommand.WorkingDirectory = directory;
            return Execute(concatCommand);
                    }
    }
}
