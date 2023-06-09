﻿using System.Text.RegularExpressions;

namespace SubtitleDubber.Helpers
{
    internal static class StringExtensions
    {
        public static string RemoveBold(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return source.Replace("<b>", string.Empty)
                .Replace("</b>", string.Empty)
                .Replace("{b}", string.Empty)
                .Replace("{/b}", string.Empty);
        }

        public static string RemoveItalic(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return source.Replace("<i>", string.Empty)
                .Replace("</i>", string.Empty)
                .Replace("{i}", string.Empty)
                .Replace("{/i}", string.Empty);
        }

        public static string RemoveUnderline(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return source.Replace("<u>", string.Empty)
                .Replace("</u>", string.Empty)
                .Replace("{u}", string.Empty)
                .Replace("{/u}", string.Empty);
        }

        public static string RemoveSpecialTags(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return source.Replace("{\\an8}", string.Empty)
                .Replace("&nbsp;", string.Empty);
        }

        public static string RemoveUnknownTags(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            try
            {
                while (source.IndexOf("<", StringComparison.Ordinal) != -1)
                {
                    var i = source.IndexOf("<", StringComparison.Ordinal);
                    var j = source.IndexOf(">", StringComparison.Ordinal);
                    source = source.Remove(i, j - i + 1);
                }
                return source;
            }
            catch
            {
                return source;
            }
        }

            public static string RemoveFont(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            var text = Regex.Replace(source, @"<font[^>]*>", string.Empty);

            return Regex.Replace(text, @"</.*font.*>", string.Empty);
        }

        public static bool ContainsReturnChar(this string source)
        {
            if (source == null)
            {
                return false;
            }

            return source.Contains("\r") || source.Contains("\n") || source.Contains("<br>") || source.Contains("<BR>");
        }

        public static bool EndsWithSentenceEndChar(this string source)
        {
if (string.IsNullOrEmpty(source))
                {
                return false;
            }
            var endChar = source[source.Length-1];

            return endChar == '.' || endChar == ',' || endChar == ';' || endChar == '!' || endChar == '-' || endChar == ':' || endChar == '\"' || endChar == '?';
        }

        public static string RemoveReturnChars(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return source.Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("<br>", " ")
                .Replace("<BR>", " ");
        }

        public static string ReplaceCommasToQuestionMarks(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return source.Replace(",", "?");
        }

        public static string RemovePunctuation(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return source.Replace(",", " ")
                .Replace("!", " ")
                .Replace(".", " ")
                .Replace("?", " ")
                .Replace("-", " ")
                .Replace(":", " ");
        }

        public static string Shorten(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            source = source.Replace("00", " ");
            source = source + " ";
for (int i=65;i<91;++i)
            {
char s = (char)i;
                source = source.Replace(" "+s+" ", " ");
                            }
            for (int i = 96; i < 123; ++i)
            {
                char s = (char)i;
                source = source.Replace(" " + s + " ", " ");
            }

            for (int i=10;i<100;++i)
                        {
                            source = source.Replace(i+string.Empty, i / 10 + " " + i % 10);
                        }
            return source;
        }

        public static string RemoveAllFormatting(this string source)
        {
            return source.RemoveBold().RemoveItalic().RemoveUnderline().RemoveFont().RemoveReturnChars().RemoveSpecialTags().RemoveUnknownTags();
        }

    }
}