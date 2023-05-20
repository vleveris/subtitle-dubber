using System;

namespace SubtitleDubber.Exceptions
{
    public class SubtitleException : Exception
    {
        public SubtitleException() : base("Error occured handling subtitle file.")
        {
        }

        public SubtitleException(string message) : base(message)
        {
        }

        public SubtitleException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}