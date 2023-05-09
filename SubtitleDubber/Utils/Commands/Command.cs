using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleDubber.Utils.Commands
{
    public abstract class Command
    {
        private string ExecutableExtention = ".exe";
        public string Executable { get; set; }
        public List<string> Arguments { get; set; }
        public string WorkingDirectory { get; set; } = string.Empty;
        public Command(string executable, List<string> arguments)
        {
            if (!ExistsOnPath(executable))
            {
                throw new ArgumentOutOfRangeException("Executable not present at PATH");
            }

            Executable = executable;
            Arguments = arguments;
        }

        public virtual void InitializeArguments()
        {

        }

        private bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        private string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
            {
                return Path.GetFullPath(fileName);
            }

            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(path, fileName + ExecutableExtention);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }

protected string EncloseFileNameWithQuotes(string fileName)
        {
            return "\"" + fileName + "\"";
        }
    }
}
