using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SubtitleDubber.Models;
using SubtitleDubber.Parsers;
using SubtitleDubber.Exceptions;

namespace SubtitleDubber.Parsers
{
    public class SSAParser : ISubtitleParser
    {
        private const string EventLine = "[Events]";
        private const char Separator = ',';
        private const string StartColumn = "Start";
        private const string EndColumn = "End";
        private const string TextColumn = "Text";
        public string FileExtension { get; set; } = ".ass|.ssa";

        public List<SubtitleItem> Parse(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            var ssaStream = new StreamReader(filePath, Encoding.UTF8).BaseStream;
            if (!ssaStream.CanRead || !ssaStream.CanSeek)
            {
                throw new IOException("Error reading file.");
            }

            ssaStream.Position = 0;

            var reader = new StreamReader(ssaStream, Encoding.UTF8, true);

            var line = reader.ReadLine();
            var lineNumber = 1;
            while (line != null && line != EventLine)
            {
                line = reader.ReadLine();
                ++lineNumber;
            }

            if (line != null)
            {
                var headerLine = reader.ReadLine();
                if (!string.IsNullOrEmpty(headerLine))
                {
                    var columnHeaders = headerLine.Split(Separator).Select(head => head.Trim()).ToList();

                    var startIndexColumn = columnHeaders.IndexOf(StartColumn);
                    var endIndexColumn = columnHeaders.IndexOf(EndColumn);
                    var textIndexColumn = columnHeaders.IndexOf(TextColumn);

                    if (startIndexColumn > 0 && endIndexColumn > 0 && textIndexColumn > 0)
                    {
                        var items = new List<SubtitleItem>();

                        line = reader.ReadLine();
                        while (line != null)
                        {
                            if (!string.IsNullOrEmpty(line))
                            {
                                var columns = line.Split(Separator);
                                var startText = columns[startIndexColumn];
                                var endText = columns[endIndexColumn];

                                var textLine = string.Join(",", columns.Skip(textIndexColumn));

                                var start = ParseSsaTimeSpan(startText);
                                var end = ParseSsaTimeSpan(endText);

                                if (!string.IsNullOrEmpty(textLine))
                                {
                                    var item = new SubtitleItem
                                    {
                                        Index = lineNumber,
                                        Duration = new SubtitleDuration(start, end),
                                        Text = textLine
                                    };
                                    items.Add(item);
                                }
                            }

                            line = reader.ReadLine();
                        }

                        if (items.Any())
                        {
                            return items;
                        }

                        throw new SubtitleException();
                    }

                    throw new SubtitleException();
                }

                throw new SubtitleException();
            }

            throw new SubtitleException();
        }

        private SubtitleTimeSpan ParseSsaTimeSpan(string stringValue)
        {
            TimeSpan result;

            if (TimeSpan.TryParse(stringValue, out result))
            {
                var subtitleTimeSpan = SubtitleTimeSpan.ConvertTimeSpanToSubtitleTimeSpan(result);
                return subtitleTimeSpan;
            }

            throw new ArgumentException("Invalid time format.");
        }
    }
}