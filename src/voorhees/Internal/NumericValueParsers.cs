namespace Voorhees.Internal {
    // Value type parser instances.  This is necessary to trick the type system into not boxing the value type results.
    public static class NumericValueParsers
    {
        internal static INumericValueParser<T> Get<T>() {
            var destinationType = typeof(T);
            if (destinationType == typeof(byte)) { return (INumericValueParser<T>)ByteValueParser.Instance; }
            if (destinationType == typeof(sbyte)) { return (INumericValueParser<T>)SByteValueParser.Instance; }
            if (destinationType == typeof(short)) { return (INumericValueParser<T>)ShortValueParser.Instance; }
            if (destinationType == typeof(ushort)) { return (INumericValueParser<T>)UShortValueParser.Instance; }
            if (destinationType == typeof(int)) { return (INumericValueParser<T>)IntValueParser.Instance; }
            if (destinationType == typeof(uint)) { return (INumericValueParser<T>)UIntValueParser.Instance; }
            if (destinationType == typeof(long)) { return (INumericValueParser<T>)LongValueParser.Instance; }
            if (destinationType == typeof(ulong)) { return (INumericValueParser<T>)ULongValueParser.Instance; }

            if (destinationType == typeof(float)) { return (INumericValueParser<T>)FloatValueParser.Instance; }
            if (destinationType == typeof(double)) { return (INumericValueParser<T>)DoubleValueParser.Instance; }
            if (destinationType == typeof(decimal)) { return (INumericValueParser<T>)DecimalValueParser.Instance; }
            return null;
        }

        internal interface INumericValueParser<T> {
            T ConvertFrom(double value);
            void WriteTo(T value, JsonTokenWriter tokenWriter);
        }

        internal class ByteValueParser : INumericValueParser<byte> {
            public static readonly ByteValueParser Instance = new();
            public byte ConvertFrom(double value) => (byte)value;
            public void WriteTo(byte value, JsonTokenWriter tokenWriter) => tokenWriter.Write(value);
        }

        internal class SByteValueParser : INumericValueParser<sbyte> {
            public static readonly SByteValueParser Instance = new();
            public sbyte ConvertFrom(double value) => (sbyte)value;
            public void WriteTo(sbyte value, JsonTokenWriter tokenWriter) => tokenWriter.Write(value);
        }

        internal class ShortValueParser : INumericValueParser<short> {
            public static readonly ShortValueParser Instance = new();
            public short ConvertFrom(double value) => (short)value;
            public void WriteTo(short value, JsonTokenWriter tokenWriter) => tokenWriter.Write(value);
        }

        internal class UShortValueParser : INumericValueParser<ushort> {
            public static readonly UShortValueParser Instance = new();
            public ushort ConvertFrom(double value) => (ushort)value;
            public void WriteTo(ushort value, JsonTokenWriter tokenWriter) => tokenWriter.Write(value);
        }

        internal class IntValueParser : INumericValueParser<int> {
            public static readonly IntValueParser Instance = new();
            public int ConvertFrom(double value) => (int)value;
            public void WriteTo(int value, JsonTokenWriter tokenWriter) => tokenWriter.Write(value);
        }

        internal class UIntValueParser : INumericValueParser<uint> {
            public static readonly UIntValueParser Instance = new();
            public uint ConvertFrom(double value) => (uint)value;
            public void WriteTo(uint value, JsonTokenWriter tokenWriter) => tokenWriter.Write(value);
        }

        internal class LongValueParser : INumericValueParser<long> {
            public static readonly LongValueParser Instance = new();
            public long ConvertFrom(double value) => (long)value;
            public void WriteTo(long value, JsonTokenWriter tokenWriter) => tokenWriter.Write(value);
        }

        internal class ULongValueParser : INumericValueParser<ulong> {
            public static readonly ULongValueParser Instance = new();
            public ulong ConvertFrom(double value) => (ulong)value;
            public void WriteTo(ulong value, JsonTokenWriter tokenWriter) => tokenWriter.Write(value);
        }

        internal class FloatValueParser : INumericValueParser<float> {
            public static readonly FloatValueParser Instance = new();
            public float ConvertFrom(double value) => (float)value;
            public void WriteTo(float value, JsonTokenWriter tokenWriter) => tokenWriter.Write(value);
        }

        internal class DoubleValueParser : INumericValueParser<double> {
            public static readonly DoubleValueParser Instance = new();
            public double ConvertFrom(double value) => value;
            public void WriteTo(double value, JsonTokenWriter tokenWriter) => tokenWriter.Write(value);
        }

        internal class DecimalValueParser : INumericValueParser<decimal> {
            public static readonly DecimalValueParser Instance = new();
            public decimal ConvertFrom(double value) => (decimal)value;
            public void WriteTo(decimal value, JsonTokenWriter tokenWriter) => tokenWriter.Write(value);
        }
    }
}