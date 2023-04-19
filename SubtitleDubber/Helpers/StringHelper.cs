using System.IO;
using System.Text;

namespace SubtitleDubber.Helpers
{
    internal class StringHelper
    {
        public static string ReadLine(string data, int lineNumber)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            using (var stringReader = new StringReader(data))
            {
                SkipLines(stringReader, lineNumber);
                return stringReader.ReadLine();
            }
        }

        public static string ReadLineToEnd(string data, int lineNumber)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            const string NewLine = "\r\n";

            using (var stringReader = new StringReader(data))
            {
                SkipLines(stringReader, lineNumber);

                var sb = new StringBuilder();

                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    sb.Append(line + NewLine);
                }

                if (sb.ToString() == string.Empty)
                {
                    return null;
                }

                return sb.ToString().TrimEnd('\r', '\n');
            }
        }

        private static void SkipLines(StringReader stringReader, int lineNumber)
        {
            for (int i = 0; i < lineNumber; i++)
            {
                stringReader.ReadLine();
            }
        }
    }
}