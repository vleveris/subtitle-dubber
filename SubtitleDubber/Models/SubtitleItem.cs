using System;
using SubtitleDubber.Helpers;
using SubtitleDubber.Exceptions;

namespace SubtitleDubber.Models
{
    public class SubtitleItem : IComparable
    {
        private const string NewLine = "\r\n";

        private int _index;
        private SubtitleDuration _duration;
        private string _text;

        public int Index
        {
            get => _index;
            set
            {
                if (!value.IsValidIndex())
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Index must be one or more.");
                }
                _index = value;
            }
        }

        public SubtitleDuration Duration
        {
            get => _duration;
            set => _duration = value ?? throw new ArgumentNullException(nameof(value), "Duration cannot be null.");
        }

        public string Text
        {
            get => _text;
            set => _text = value == null ? string.Empty : value.Trim();
        }

public SubtitleItem()
        {
                    }

        public SubtitleItem(int index, SubtitleDuration duration, string text)
        {
            Index = index;
            Duration = duration;
            Text = text;
        }

        public SubtitleItem(string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                throw new ArgumentException(nameof(item));
            }
            item = item.Trim();

            try
            {
                Index = int.Parse(StringHelper.ReadLine(item, 0));
                Duration = new SubtitleDuration(StringHelper.ReadLine(item, 1));
                Text = StringHelper.ReadLineToEnd(item, 2);
            }
            catch (Exception ex)
            {
                throw new SubtitleException("Invalid subtitle item representation.", ex);
            }
        }

                public override string ToString()
                {
                    return _index + NewLine +
                        Duration + NewLine +
                        Text;
                }

                public override int GetHashCode()
                {
                    return (_index + Text).GetHashCode();
                }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            var subtitle = (SubtitleItem)obj;

            return _index.CompareTo(subtitle._index);
        }

        #endregion

    }
}