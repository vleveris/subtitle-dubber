namespace SubtitleDubber.Helpers
{
    internal static class IntExtensions
    {
        public static bool IsValidHours(this int source)
        {
            return source >= 0 && source <= 23;
        }

        public static bool IsValidMinutes(this int source)
        {
            return source >= 0 && source <= 59;
        }

        public static bool IsValidSeconds(this int source)
        {
            return source >= 0 && source <= 59;
        }

        public static bool IsValidMilliseconds(this int source)
        {
            return source >= 0 && source <= 999;
        }

        public static bool IsValidIndex(this int source)
        {
            return source >=1;
        }
    }
}