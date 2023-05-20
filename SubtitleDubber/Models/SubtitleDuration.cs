using System;

namespace SubtitleDubber.Models
{
    public class SubtitleDuration
    {
        private const string TimeDelimiter = " --> ";

        private static readonly int ValidStringLength = (SubtitleTimeSpan.ValidStringLength * 2) + TimeDelimiter.Length;

        public SubtitleTimeSpan Start { get; set; }

        public SubtitleTimeSpan End { get; set; }

        public SubtitleTimeSpan TotalActiveTime => End.Subtract(Start);

        public SubtitleDuration(string duration)
        {
            if (!IsValidDuration(duration))
            {
                throw new ArgumentException($"Duration string: '{duration}' is in invalid format.");
            }
            var start = new SubtitleTimeSpan(duration.Substring(0, SubtitleTimeSpan.ValidStringLength));
            var end = new SubtitleTimeSpan(duration.Substring(SubtitleTimeSpan.ValidStringLength + TimeDelimiter.Length));

            if (!IsValidStartAndEnd(start, end))
            {
                throw new ArgumentException("Start time is after end time.");
            }
            Start = start;
            End = end;
        }

        public SubtitleDuration(SubtitleTimeSpan start, SubtitleTimeSpan end)
        {
            if (!IsValidStartAndEnd(start, end))
            {
                throw new ArgumentException("Start time is after end time.");
            }
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return Start + TimeDelimiter + End;
        }

        private static bool IsValidDuration(string duration)
        {
            if (duration?.Length != ValidStringLength)
            {
                return false;
            }
            if (duration.Substring(12, TimeDelimiter.Length) != TimeDelimiter)
            {
                return false;
            }
            return true;
        }

        private static bool IsValidStartAndEnd(SubtitleTimeSpan start, SubtitleTimeSpan end)
        {
            return start.TotalMilliseconds <= end.TotalMilliseconds;
        }
    }
}
