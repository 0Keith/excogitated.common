﻿using Excogitated.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Excogitated.Extensions
{
    public static class Extensions_String
    {
        public static bool EqualsIgnoreCase(this string value, string other) => value.Equals(other, StringComparison.CurrentCultureIgnoreCase);
        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);
        public static bool IsNotNullOrWhiteSpace(this string value) => !value.IsNullOrWhiteSpace();
        public static bool IsWhiteSpace(this char c) => char.IsWhiteSpace(c);
        public static bool IsLetter(this char c) => char.IsLetter(c);
        public static bool IsDigit(this char c) => char.IsDigit(c);
        public static int ToInt(this char c) => c - '0';
        public static char ToChar(this int i) => (char)(i + '0');

        public static StringBuilder AppendCSV(this StringBuilder csv, object value = null)
        {
            if (csv != null)
                csv.Append('"').Append(value).Append('"').Append(',');
            return csv;
        }

        private static readonly HashSet<char> _invalidPathChars = Path.GetInvalidFileNameChars().ToHashSet();
        public static string EscapeFileName(this string path)
        {
            if (path is null)
                return string.Empty;
            var b = new StringBuilder(path.Length);
            foreach (var c in path)
                if (c == '&' || _invalidPathChars.Contains(c))
                    b.Append('&').Append((int)c).Append('&');
                else
                    b.Append(c);
            return b.ToString();
        }

        public static string UnescapeFileName(this string path)
        {
            if (path is null)
                return string.Empty;
            var b = new StringBuilder(path.Length);
            using (var chars = path.GetEnumerator())
                while (chars.MoveNext())
                {
                    var c = chars.Current;
                    if (c != '&')
                        b.Append(c);
                    else
                    {
                        var i = 0;
                        while (chars.MoveNext())
                        {
                            c = chars.Current;
                            if (c == '&')
                                break;
                            else
                            {
                                i *= 10;
                                i += c.ToInt();
                            }
                        }
                        b.Append((char)i);
                    }
                }
            return b.ToString();
        }

        public static IEnumerable<int> IndexesOf(this string source, char value)
        {
            if (source is null == false)
                for (var i = 0; i < source.Length; i++)
                    if (source[i] == value)
                        yield return i;
        }

        public static IEnumerable<int> IndexesOf(this string source, Func<char, bool> where)
        {
            if (source is null == false && where is null == false)
                for (var i = 0; i < source.Length; i++)
                    if (where(source[i]))
                        yield return i;
        }

        public static IEnumerable<int> GetNumericParts(this string value)
        {
            if (value is null == false)
            {
                var part = 0;
                foreach (var c in value)
                {
                    var digit = c.ToInt();
                    if (digit >= 0 && digit <= 9)
                    {
                        part *= 10;
                        part += digit;
                    }
                    else
                    {
                        yield return part;
                        part = 0;
                    }
                }
                yield return part;
            }
        }

        public static int GetDigits(this string value)
        {
            var digits = 0;
            if (value is null == false)
                foreach (var c in value)
                {
                    var digit = c.ToInt();
                    if (digit >= 0 && digit <= 9)
                    {
                        digits *= 10;
                        digits += digit;
                    }
                }
            return digits;
        }

        public static string AsString(this IEnumerable<char> source)
        {
            if (source is null)
                return null;
            return new string(source.ToArray());
        }

        public static IEnumerable<string> GetCharacterNames(this IEnumerable<char> source)
        {
            if (source is null)
                return new string[0];
            return source.Select(c => c switch
            {
                '0' => "Zero",
                '1' => "One",
                '2' => "Two",
                '3' => "Three",
                '4' => "Four",
                '5' => "Five",
                '6' => "Six",
                '7' => "Seven",
                '8' => "Eight",
                '9' => "Nine",
                '_' => "Underscore",
                '-' => "Dash",
                '|' => "Pipe",
                _ => "Unknown",
            });
        }

        public static DiffResult Diff(this IEnumerable<char> expected, IEnumerable<char> actual, bool ignoreWhitespace = false, int diffLength = 100)
        {
            if (expected is null)
                expected = string.Empty;
            if (actual is null)
                actual = string.Empty;
            if (ignoreWhitespace)
            {
                expected = expected.Where(c => !c.IsWhiteSpace());
                actual = actual.Where(c => !c.IsWhiteSpace());
            }

            var zip = expected.Zip(actual, (e, a) => (Expected: e, Actual: a)).SkipWhile(z => z.Expected == z.Actual).Take(diffLength).ToList();
            if (zip.Count > 0)
                return new DiffResult
                {
                    DifferenceFound = true,
                    Expected = zip.Select(z => z.Expected).AsString(),
                    Actual = zip.Select(z => z.Actual).AsString(),
                };
            return default;
        }

        public static string ToCamelCase(this string value, string separator = " ")
        {
            if (value is null)
                value = string.Empty;
            if (separator is null)
                separator = string.Empty;
            var separated = false;
            var b = new StringBuilder(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (i == 0 && char.IsLower(c))
                    c = char.ToUpper(c);
                if (!char.IsLetterOrDigit(c))
                {
                    separated = true;
                    continue;
                }
                if (separated)
                {
                    b.Append(separator);
                    if (char.IsLower(c))
                        c = char.ToUpper(c);
                    separated = false;
                }
                b.Append(c);
            }
            return b.ToString();
        }
    }

    public struct DiffResult
    {
        public bool DifferenceFound { get; set; }
        public string Expected { get; set; }
        public string Actual { get; set; }

        public override string ToString() => $"Different: {DifferenceFound}, Expected: {Expected}, Actual: {Actual}";
    }
}
