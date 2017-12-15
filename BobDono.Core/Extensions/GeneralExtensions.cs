using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BobDono.Core.Extensions
{
    public static class GeneralExtensions
    {
        private static readonly Random Rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static bool IsLink(this string s)
        {
            return Regex.IsMatch(s,
                @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)");
        }

        public static DateTime GetNextElectionDate(this DateTime from)
        {
            if (from.Day >= 1 && from.Day < 15)
            {
                from = new DateTime(from.Year, from.Month, 15);
            }
            else
            {
                from = new DateTime(from.Year, from.Month, 1);
                from = from.AddMonths(1);
            }
            return from;


        }

        public static List<string> WordWrap(this string input, int maxCharacters)
        {
            List<string> lines = new List<string>();

            if (!input.Contains(" "))
            {
                int start = 0;
                while (start < input.Length)
                {
                    lines.Add(input.Substring(start, Math.Min(maxCharacters, input.Length - start)));
                    start += maxCharacters;
                }
            }
            else
            {
                string[] words = input.Split(' ');

                string line = "";
                foreach (string word in words)
                {
                    if ((line + word).Length > maxCharacters)
                    {
                        lines.Add(line.Trim());
                        line = "";
                    }

                    line += string.Format("{0} ", word);
                }

                if (line.Length > 0)
                {
                    lines.Add(line.Trim());
                }
            }

            return lines;
        }

        //static char[] splitChars = new char[] { ' ', '-', '\t' };

        //public static string WordWrap(this string str, int width)
        //{
        //    string[] words = Explode(str, splitChars);

        //    int curLineLength = 0;
        //    StringBuilder strBuilder = new StringBuilder();
        //    for (int i = 0; i < words.Length; i += 1)
        //    {
        //        string word = words[i];
        //        // If adding the new word to the current line would be too long,
        //        // then put it on a new line (and split it up if it's too long).
        //        if (curLineLength + word.Length > width)
        //        {
        //            // Only move down to a new line if we have text on the current line.
        //            // Avoids situation where wrapped whitespace causes emptylines in text.
        //            if (curLineLength > 0)
        //            {
        //                strBuilder.Append(Environment.NewLine);
        //                curLineLength = 0;
        //            }

        //            // If the current word is too long to fit on a line even on it's own then
        //            // split the word up.
        //            while (word.Length > width)
        //            {
        //                strBuilder.Append(word.Substring(0, width - 1) + "-");
        //                word = word.Substring(width - 1);

        //                strBuilder.Append(Environment.NewLine);
        //            }

        //            // Remove leading whitespace from the word so the new line starts flush to the left.
        //            word = word.TrimStart();
        //        }
        //        strBuilder.Append(word);
        //        curLineLength += word.Length;
        //    }

        //    return strBuilder.ToString();
        //}

        //private static string[] Explode(string str, char[] splitChars)
        //{
        //    List<string> parts = new List<string>();
        //    int startIndex = 0;
        //    while (true)
        //    {
        //        int index = str.IndexOfAny(splitChars, startIndex);

        //        if (index == -1)
        //        {
        //            parts.Add(str.Substring(startIndex));
        //            return parts.ToArray();
        //        }

        //        string word = str.Substring(startIndex, index - startIndex);
        //        char nextChar = str.Substring(index, 1)[0];
        //        // Dashes and the likes should stick to the word occuring before it. Whitespace doesn't have to.
        //        if (char.IsWhiteSpace(nextChar))
        //        {
        //            parts.Add(word);
        //            parts.Add(nextChar.ToString());
        //        }
        //        else
        //        {
        //            parts.Add(word + nextChar);
        //        }

        //        startIndex = index + 1;
        //    }
        //}
    }
}
