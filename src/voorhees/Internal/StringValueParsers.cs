using System;

namespace Voorhees.Internal {
    public static class StringValueParsers {
        internal static IStringValueParser<T> Get<T>() {
            var destinationType = typeof(T);
            if (destinationType == typeof(char)) { return (IStringValueParser<T>)CharValueParser.Instance; }
            if (destinationType == typeof(DateTime)) { return (IStringValueParser<T>)DateTimeValueParser.Instance; }
            if (destinationType == typeof(DateTimeOffset)) { return (IStringValueParser<T>)DateTimeOffsetValueParser.Instance; }
            return null;
        }

        internal interface IStringValueParser<out T> {
            T Parse(string str);
        }

        internal class CharValueParser : IStringValueParser<char> {
            public static readonly CharValueParser Instance = new();

            public char Parse(string str) {
                if (str.Length != 1) {
                    // TODO throw a better exception type here.
                    throw new FormatException($"Trying to map a string of length != 1 to a char: \"{str}\"");
                }
                return str[0];
            }
        }

        internal class DateTimeValueParser : IStringValueParser<DateTime> {
            public static readonly DateTimeValueParser Instance = new();

            public DateTime Parse(string str) {
                return DateTime.Parse(str);
            }
        }

        internal class DateTimeOffsetValueParser : IStringValueParser<DateTimeOffset> {
            public static readonly DateTimeOffsetValueParser Instance = new();

            public DateTimeOffset Parse(string str) {
                return DateTimeOffset.Parse(str);
            }
        }
    }
}
