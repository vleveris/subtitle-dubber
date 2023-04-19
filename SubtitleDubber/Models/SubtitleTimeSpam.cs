using System;
using System.Text.RegularExpressions;
using SubtitleDubber.Helpers;
namespace SubtitleDubber.Models
{
    public readonly struct SubtitleTimeSpan : IEquatable<SubtitleTimeSpan>
    {
        private readonly TimeSpan _timeSpan;

        internal const int ValidStringLength = 12;

        public int Hours => _timeSpan.Hours;

        public int Minutes => _timeSpan.Minutes;

        public int Seconds => _timeSpan.Seconds;

        public int Milliseconds => _timeSpan.Milliseconds;

        public double TotalHours => _timeSpan.TotalHours;

        public double TotalMinutes => _timeSpan.TotalMinutes;

        public double TotalSeconds => _timeSpan.TotalSeconds;

        public double TotalMilliseconds => _timeSpan.TotalMilliseconds;

        public SubtitleTimeSpan(string timeSpan)
        {
            if (!IsValidFormat(timeSpan))
            {
                throw new ArgumentException("Time span must be in the format: HH:MM:SS,MMM. Max value: 23:59:59:999.", nameof(timeSpan));
            }

            _timeSpan = new TimeSpan(0,
                GetHours(timeSpan),
                GetMinutes(timeSpan),
                GetSeconds(timeSpan),
                GetMilliseconds(timeSpan));
        }

        public SubtitleTimeSpan(int hours, int minutes, int seconds, int milliseconds)
        {
            if (!hours.IsValidHours())
            {
                throw new ArgumentOutOfRangeException(nameof(hours), hours, "Hours must be between 0 and 23.");
            }

            if (!minutes.IsValidMinutes())
            {
                throw new ArgumentOutOfRangeException(nameof(minutes), minutes, "Minutes must be between 0 and 59.");
            }

            if (!seconds.IsValidSeconds())
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Seconds must be between 0 and 59.");
            }

            if (!milliseconds.IsValidMilliseconds())
            {
                throw new ArgumentOutOfRangeException(nameof(milliseconds), milliseconds, "Milliseconds must be between 0 and 999.");
            }

            _timeSpan = new TimeSpan(0, hours, minutes, seconds, milliseconds);
        }

        public static SubtitleTimeSpan MaxValue() => new SubtitleTimeSpan(23, 59, 59, 999);

        public static SubtitleTimeSpan MinValue() => new SubtitleTimeSpan(0, 0, 0, 0);

        public SubtitleTimeSpan Add(SubtitleTimeSpan subtitleTimeSpan)
        {
            var ts = ConvertSrtTimeSpanToTimeSpan(subtitleTimeSpan);

            ts = _timeSpan.Add(ts);

            return ConvertTimeSpanToSrtTimeSpan(ts);
        }

        public SubtitleTimeSpan Subtract(SubtitleTimeSpan subtitleTimeSpan)
        {
            var ts = ConvertSrtTimeSpanToTimeSpan(subtitleTimeSpan);

            ts = _timeSpan.Subtract(ts);

            if (ts.TotalMilliseconds < 0)
            {
                return new SubtitleTimeSpan(0, 0, 0, 0);
            }

            return ConvertTimeSpanToSrtTimeSpan(ts);
        }

        public override string ToString()
        {
            return _timeSpan.Hours.ToString("00") + ":" +
                _timeSpan.Minutes.ToString("00") + ":" +
                _timeSpan.Seconds.ToString("00") + "," +
                _timeSpan.Milliseconds.ToString("000");
        }

        #region Comparison methods

        public bool Equals(SubtitleTimeSpan other)
        {
            return _timeSpan.Equals(other._timeSpan);
        }

        public override bool Equals(object obj)
        {
            return obj is SubtitleTimeSpan other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _timeSpan.GetHashCode();
        }

        public static bool operator ==(SubtitleTimeSpan left, SubtitleTimeSpan right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SubtitleTimeSpan left, SubtitleTimeSpan right)
        {
            return !left.Equals(right);
        }

        public static bool operator >(SubtitleTimeSpan left, SubtitleTimeSpan right)
        {
            return left.TotalMilliseconds > right.TotalMilliseconds;
        }

        public static bool operator <(SubtitleTimeSpan left, SubtitleTimeSpan right)
        {
            return left.TotalMilliseconds < right.TotalMilliseconds;
        }

        public static bool operator >=(SubtitleTimeSpan left, SubtitleTimeSpan right)
        {
            return left.TotalMilliseconds >= right.TotalMilliseconds;
        }

        public static bool operator <=(SubtitleTimeSpan left, SubtitleTimeSpan right)
        {
            return left.TotalMilliseconds <= right.TotalMilliseconds;
        }

        #endregion

        internal static bool IsValidFormat(string timeSpan)
        {
            if (timeSpan == null)
            {
                return false;
            }

            // Valid format: "00:00:00,000"
            return Regex.IsMatch(timeSpan, "^[0-2][0-3]:[0-5][0-9]:[0-5][0-9],[0-9][0-9][0-9]$");
        }

        private static int GetHours(string ts)
        {
            return int.Parse(ts.Substring(0, 2));
        }

        private static int GetMinutes(string ts)
        {
            return int.Parse(ts.Substring(3, 2));
        }

        private static int GetSeconds(string ts)
        {
            return int.Parse(ts.Substring(6, 2));
        }

        private static int GetMilliseconds(string ts)
        {
            return int.Parse(ts.Substring(9, 3));
        }

        private static TimeSpan ConvertSrtTimeSpanToTimeSpan(SubtitleTimeSpan subtitleTimeSpan)
        {
            return new TimeSpan(0, subtitleTimeSpan.Hours, subtitleTimeSpan.Minutes, subtitleTimeSpan.Seconds, subtitleTimeSpan.Milliseconds);
        }

        private static SubtitleTimeSpan ConvertTimeSpanToSrtTimeSpan(TimeSpan timeSpan)
        {
            return new SubtitleTimeSpan(timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }
    }
}