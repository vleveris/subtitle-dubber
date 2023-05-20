using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SubtitleDubber.Models;
using System.Text.RegularExpressions;
using SubtitleDubber.Exceptions;

namespace SubtitleDubber.Parsers
{
    public class VTTParser : ISubtitleParser
    {
        private readonly string[] Delimiters = { "-->", "- >", "->" };
        public string FileExtension { get; set; } = ".vtt";

        public List<SubtitleItem> Parse(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            var vttStream = new StreamReader(filePath, Encoding.UTF8).BaseStream;
            if (!vttStream.CanRead || !vttStream.CanSeek)
            {
                throw new IOException("Error reading file.");
            }

            vttStream.Position = 0;

            var reader = new StreamReader(vttStream, Encoding.UTF8, true);

            var items = new List<SubtitleItem>();
            var vttSubParts = GetVttSubTitleParts(reader).ToList();
            if (vttSubParts.Any())
            {
                int index = 1;
                foreach (var vttSubPart in vttSubParts)
                {
                    var lines =
                        vttSubPart.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                            .Select(s => s.Trim())
                            .Where(l => !string.IsNullOrEmpty(l))
                            .ToList();

                    var item = new SubtitleItem();
                    foreach (var line in lines)
                    {
                        if (item.Duration == null)
                        {
                            SubtitleDuration duration;
                            var success = TryParseDurationLine(line, out duration);
                            if (success)
                            {
                                item.Duration = duration;
                            }
                        }
                        else
                        {
                            item.Text = line;
                        }

                        item.Text = string.IsNullOrEmpty(item.Text) ? "" : item.Text;
                    }

                    if (item.Duration != null && item.Text.Any())
                    {
                        item.Index = index;
                        ++index;
                        items.Add(item);
                    }
                }

                return items;
            }
            throw new SubtitleException();
        }

        private IEnumerable<string> GetVttSubTitleParts(TextReader reader)
        {
            string line;
            var sb = new StringBuilder();

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line.Trim()))
                {
                    var res = sb.ToString().TrimEnd();
                    if (!string.IsNullOrEmpty(res))
                    {
                        yield return res;
                    }

                    sb = new StringBuilder();
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            if (sb.Length > 0)
            {
                yield return sb.ToString();
            }
        }

        private bool TryParseDurationLine(string line, out SubtitleDuration duration)
        {
            var parts = line.Split(Delimiters, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                duration = null;
                return false;
            }
            duration = new SubtitleDuration(ParseVttTimeSpan(parts[0]), ParseVttTimeSpan(parts[1]));
            return true;
        }

        private SubtitleTimeSpan ParseVttTimeSpan(string stringValue)
        {
            var timeString = string.Empty;
            var match = Regex.Match(stringValue, "[0-9]+:[0-9]+:[0-9]+[,\\.][0-9]+");
            if (match.Success)
            {
                timeString = match.Value;
            }
            else
            {
                match = Regex.Match(stringValue, "[0-9]+:[0-9]+[,\\.][0-9]+");
                if (match.Success)
                {
                    timeString = "00:" + match.Value;
                }
            }

            if (!string.IsNullOrEmpty(timeString))
            {
                timeString = timeString.Replace(',', '.');
                TimeSpan result;
                if (TimeSpan.TryParse(timeString, out result))
                {
                    var subtitleTimeSpan = SubtitleTimeSpan.ConvertTimeSpanToSubtitleTimeSpan(result);
                    return subtitleTimeSpan;
                }
            }
            throw new ArgumentException("Invalid time format.");
        }
    }
}