using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#nullable enable

namespace VoidNullEngine.Engine.Math
{
    public readonly struct Rational : IEquatable<Rational>, IComparable, IComparable<Rational>, IConvertible, IFormattable
    {
        #region Constants
        public static readonly Rational NaN = new(BigInteger.Zero, BigInteger.Zero);
        public static readonly Rational NegativeInfinity = new(BigInteger.MinusOne, BigInteger.Zero);
        public static readonly Rational PositiveInfinity = new(BigInteger.One, BigInteger.Zero);
        public static readonly Rational Zero = new(BigInteger.Zero, BigInteger.One);
        public static readonly Rational One = new(BigInteger.One, BigInteger.One);
        public static readonly Rational MinusOne = new(BigInteger.MinusOne, BigInteger.One);
        #endregion
        #region Instance Data
        private readonly BigInteger numerator;
        private readonly BigInteger? denominator;

        private BigInteger Numerator => numerator;
        private BigInteger Denominator => denominator ?? 1;

        public bool IsFinite => !Denominator.IsZero;
        public bool IsNaN => !IsFinite && Numerator.IsZero;
        public bool IsInfinite => !IsFinite && !Numerator.IsZero;
        public bool IsZero => IsFinite && Numerator.IsZero;
        public bool IsNormal => IsFinite && Numerator.Sign > 0;
        public bool IsSubnormal => IsFinite && Numerator.Sign < 0;
        public int Sign => Numerator.Sign;

        public Rational(BigInteger numerator, BigInteger denominator)
        {
            if (denominator.IsZero)
            {
                this.numerator = numerator.Sign;
                this.denominator = denominator;
                return;
            }

            Reduce(ref numerator, ref denominator);
            if (denominator.Sign < 0) numerator = -numerator;
            this.numerator = numerator;
            this.denominator = BigInteger.Abs(denominator);
        }

        #endregion
        #region Conversions
        public static implicit operator Rational(int intValue) => new(intValue, BigInteger.One);
        public static implicit operator Rational(long longValue) => new(longValue, BigInteger.One);
        public static implicit operator Rational(BigInteger bigValue) => new(bigValue, BigInteger.One);
        public static implicit operator Rational(double doubleValue)
        {
            if (double.IsNaN(doubleValue)) return NaN;
            if (double.IsPositiveInfinity(doubleValue)) return PositiveInfinity;
            if (double.IsNegativeInfinity(doubleValue)) return NegativeInfinity;

            const ulong SIGN_BIT = 0x8000000000000000;
            const ulong EXPONENT_BITS = 0x7FF0000000000000;
            const ulong MANTISSA = 0x000FFFFFFFFFFFFF;
            const ulong MANTISSA_DIVISOR = 0x0010000000000000;
            const ulong K = 1023;

            ulong valueBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(doubleValue));

            if (valueBits == 0) return Zero;

            bool negative = (valueBits & SIGN_BIT) == SIGN_BIT;
            ulong mantissaBits = valueBits & MANTISSA;

            int exponent = (int)(((valueBits & EXPONENT_BITS) >> 52) - K);

            BigInteger numerator = mantissaBits + MANTISSA_DIVISOR;
            BigInteger denominator = MANTISSA_DIVISOR;
            BigInteger power = BigInteger.One << exponent;

            if (exponent < 0) denominator *= power;
            else numerator *= power;

            if (negative) numerator = -numerator;

            return new(numerator, denominator);
        }

        public TypeCode GetTypeCode() => TypeCode.Object;

        public object ToType(Type conversionType, IFormatProvider? provider)
        {
            if (typeof(sbyte) == conversionType) return ToSByte(provider);
            if (typeof(byte) == conversionType) return ToByte(provider);
            if (typeof(short) == conversionType) return ToInt16(provider);
            if (typeof(ushort) == conversionType) return ToUInt16(provider);
            if (typeof(int) == conversionType) return ToInt32(provider);
            if (typeof(uint) == conversionType) return ToUInt32(provider);
            if (typeof(long) == conversionType) return ToInt64(provider);
            if (typeof(ulong) == conversionType) return ToUInt64(provider);
            if (typeof(float) == conversionType) return ToSingle(provider);
            if (typeof(double) == conversionType) return ToDouble(provider);
            if (typeof(decimal) == conversionType) return ToDecimal(provider);
            if (typeof(string) == conversionType) return ToString(provider);
            if (typeof(BigInteger) == conversionType && IsFinite) return Numerator / Denominator;
            throw new InvalidCastException();
        }

        public bool ToBoolean(IFormatProvider? provider) => throw new InvalidCastException();

        public char ToChar(IFormatProvider? provider) => throw new InvalidCastException();

        public DateTime ToDateTime(IFormatProvider? provider) => throw new InvalidCastException();

        public sbyte ToSByte(IFormatProvider? provider)
        {
            if (!IsFinite) throw new InvalidCastException();
            if (IsZero) return 0;
            return (sbyte)(Numerator / Denominator);
        }

        public byte ToByte(IFormatProvider? provider)
        {
            if (!IsFinite) throw new InvalidCastException();
            if (IsZero) return 0;
            return (byte)(Numerator / Denominator);
        }

        public short ToInt16(IFormatProvider? provider)
        {
            if (!IsFinite) throw new InvalidCastException();
            if (IsZero) return 0;
            return (short)(Numerator / Denominator);
        }

        public ushort ToUInt16(IFormatProvider? provider)
        {
            if (!IsFinite) throw new InvalidCastException();
            if (IsZero) return 0;
            return (ushort)(Numerator / Denominator);
        }

        public int ToInt32(IFormatProvider? provider)
        {
            if (!IsFinite) throw new InvalidCastException();
            if (IsZero) return 0;
            return (int)(Numerator / Denominator);
        }

        public uint ToUInt32(IFormatProvider? provider)
        {
            if (!IsFinite) throw new InvalidCastException();
            if (IsZero) return 0;
            return (uint)(Numerator / Denominator);
        }

        public long ToInt64(IFormatProvider? provider)
        {
            if (!IsFinite) throw new InvalidCastException();
            if (IsZero) return 0;
            return (long)(Numerator / Denominator);
        }

        public ulong ToUInt64(IFormatProvider? provider)
        {
            if (!IsFinite) throw new InvalidCastException();
            if (IsZero) return 0;
            return (ulong)(Numerator / Denominator);
        }

        public float ToSingle(IFormatProvider? provider)
        {
            if (!IsFinite) return Sign switch { -1 => float.NegativeInfinity, 1 => float.PositiveInfinity, _ => float.NaN };
            if (IsZero) return 0;
            return (float)Numerator / (float)Denominator;
        }

        public double ToDouble(IFormatProvider? provider)
        {
            if (!IsFinite) return Sign switch { -1 => double.NegativeInfinity, 1 => double.PositiveInfinity, _ => double.NaN };
            if (IsZero) return 0;
            return (double)Numerator / (double)Denominator;
        }

        private static readonly BigInteger MIN_DECIMAL = new(decimal.MinValue), MAX_DECIMAL = new(decimal.MaxValue);
        public decimal ToDecimal(IFormatProvider? provider)
        {
            if (!IsFinite) throw new InvalidCastException();
            if (IsZero) return decimal.Zero;

            if (Numerator >= MIN_DECIMAL && Numerator <= MAX_DECIMAL && Denominator >= MIN_DECIMAL && Denominator <= MAX_DECIMAL)
            {
                return (decimal)Numerator / (decimal)Denominator;
            }

            var i = (decimal)(Numerator / Denominator);

            var p = BigInteger.Pow(10, 28);
            var rem = Numerator % Denominator;
            var dec = (decimal)(rem * p / Denominator) / (decimal)p;

            return i + dec;
        }

        public override string ToString() => ToString("r", CultureInfo.CurrentCulture);

        public string ToString(IFormatProvider? provider) => ToString("r", provider);

        public string ToString(string? format, IFormatProvider? provider)
        {
            format ??= "I";
            provider ??= CultureInfo.CurrentCulture;
            format = format.ToUpperInvariant();
            var info = (provider as CultureInfo)?.NumberFormat ?? (provider as NumberFormatInfo) ?? NumberFormatInfo.CurrentInfo;

            char type = format[0];
            int? precision = null;
            if (format.Length > 1) precision = int.Parse(format[1..]);
            if (precision is not null && precision < 0) throw new FormatException();

            if (IsNaN) return info.NaNSymbol;
            if (IsInfinite && Sign > 0) return info.PositiveInfinitySymbol;
            if (IsInfinite) return info.NegativeInfinitySymbol;

            switch (type)
            {
                case 'C':
                    {
                        precision ??= info.CurrencyDecimalDigits;
                        
                    }
                    break;
                case 'I':
                    return $"{Numerator.ToString(provider)}/{Denominator.ToString(provider)}";
            }
            throw new FormatException();
        }

        #endregion
        #region Utility Methods
        private static void Root(in Rational @base, in int n, ref BigInteger num, ref BigInteger den)
        {
            var a = @base.Numerator * BigInteger.Pow(den, n);

            var y = BigInteger.Pow(num, n - 1);
            den *= @base.Denominator * n * y;
            num = y * num * (n - @base.Denominator) + a;

            Reduce(ref num, ref den);
        }

        private static BigInteger LowestCommonMultiple(in BigInteger a, in BigInteger b)
        {
            if (a.IsZero && b.IsZero) return 0;
            return a / BigInteger.GreatestCommonDivisor(a, b) * b;
        }

        private static void Reduce(ref BigInteger n, ref BigInteger d)
        {
            BigInteger gcd = BigInteger.GreatestCommonDivisor(n, d);
            n /= gcd;
            d /= gcd;
        }
        #endregion
        #region Operations
        public bool Equals(Rational other) => CompareTo(other) == 0;

        public override bool Equals(object? obj)
        {
            if (obj is Rational rational) return Equals(rational);
            if (obj is IConvertible convertible)
            {
                try
                {
                    return convertible.GetTypeCode() switch
                    {
                        TypeCode.SByte => ToSByte(CultureInfo.InvariantCulture).Equals(convertible.ToSByte(CultureInfo.InvariantCulture)),
                        TypeCode.Byte => ToByte(CultureInfo.InvariantCulture).Equals(convertible.ToByte(CultureInfo.InvariantCulture)),
                        TypeCode.Int16 => ToInt16(CultureInfo.InvariantCulture).Equals(convertible.ToInt16(CultureInfo.InvariantCulture)),
                        TypeCode.UInt16 => ToUInt16(CultureInfo.InvariantCulture).Equals(convertible.ToUInt16(CultureInfo.InvariantCulture)),
                        TypeCode.Int32 => ToInt32(CultureInfo.InvariantCulture).Equals(convertible.ToInt32(CultureInfo.InvariantCulture)),
                        TypeCode.UInt32 => ToUInt32(CultureInfo.InvariantCulture).Equals(convertible.ToUInt32(CultureInfo.InvariantCulture)),
                        TypeCode.Int64 => ToInt64(CultureInfo.InvariantCulture).Equals(convertible.ToInt64(CultureInfo.InvariantCulture)),
                        TypeCode.UInt64 => ToUInt64(CultureInfo.InvariantCulture).Equals(convertible.ToUInt64(CultureInfo.InvariantCulture)),
                        TypeCode.Single => ToSingle(CultureInfo.InvariantCulture).Equals(convertible.ToSingle(CultureInfo.InvariantCulture)),
                        TypeCode.Double => ToDouble(CultureInfo.InvariantCulture).Equals(convertible.ToDouble(CultureInfo.InvariantCulture)),
                        TypeCode.Decimal => ToDecimal(CultureInfo.InvariantCulture).Equals(convertible.ToDecimal(CultureInfo.InvariantCulture)),
                        _ => false,
                    };
                }
                catch (InvalidCastException)
                {
                    return false;
                }
            }
            return false;
        }

        public override int GetHashCode() => HashCode.Combine(typeof(Rational), Numerator, Denominator);

        public int CompareTo(Rational other)
        {
            if (IsNaN) return other.IsNaN ? 0 : -1;
            if (other.IsNaN) return 1;

            if (Sign != other.Sign) return Sign;

            if (IsInfinite) return other.IsFinite ? Sign : 0;
            if (other.IsInfinite) return other.Sign;

            var x = LowestCommonMultiple(Denominator, other.Denominator);
            var ay = x / Denominator * Numerator;
            var by = x / other.Denominator * other.Numerator;
            var y = ay - by;

            return y.Sign;
        }

        public int CompareTo(object? obj)
        {
            if (obj is Rational rational) return CompareTo(rational);
            if (obj is IConvertible convertible)
            {
                return convertible.GetTypeCode() switch
                {
                    TypeCode.SByte => IsFinite ? ToSByte(CultureInfo.InvariantCulture).CompareTo(convertible.ToSByte(CultureInfo.InvariantCulture)) : IsNaN ? -1 : Sign,
                    TypeCode.Byte => IsFinite ? ToByte(CultureInfo.InvariantCulture).CompareTo(convertible.ToByte(CultureInfo.InvariantCulture)) : IsNaN ? -1 : Sign,
                    TypeCode.Int16 => IsFinite ? ToInt16(CultureInfo.InvariantCulture).CompareTo(convertible.ToInt16(CultureInfo.InvariantCulture)) : IsNaN ? -1 : Sign,
                    TypeCode.UInt16 => IsFinite ? ToUInt16(CultureInfo.InvariantCulture).CompareTo(convertible.ToUInt16(CultureInfo.InvariantCulture)) : IsNaN ? -1 : Sign,
                    TypeCode.Int32 => IsFinite ? ToInt32(CultureInfo.InvariantCulture).CompareTo(convertible.ToInt32(CultureInfo.InvariantCulture)) : IsNaN ? -1 : Sign,
                    TypeCode.UInt32 => IsFinite ? ToUInt32(CultureInfo.InvariantCulture).CompareTo(convertible.ToUInt32(CultureInfo.InvariantCulture)) : IsNaN ? -1 : Sign,
                    TypeCode.Int64 => IsFinite ? ToInt64(CultureInfo.InvariantCulture).CompareTo(convertible.ToInt64(CultureInfo.InvariantCulture)) : IsNaN ? -1 : Sign,
                    TypeCode.UInt64 => IsFinite ? ToUInt64(CultureInfo.InvariantCulture).CompareTo(convertible.ToUInt64(CultureInfo.InvariantCulture)) : IsNaN ? -1 : Sign,
                    TypeCode.Single => ToSingle(CultureInfo.InvariantCulture).CompareTo(convertible.ToSingle(CultureInfo.InvariantCulture)),
                    TypeCode.Double => ToDouble(CultureInfo.InvariantCulture).CompareTo(convertible.ToDouble(CultureInfo.InvariantCulture)),
                    TypeCode.Decimal => ToDecimal(CultureInfo.InvariantCulture).CompareTo(convertible.ToDecimal(CultureInfo.InvariantCulture)),
                    _ => throw new ArgumentException($"Type of {nameof(obj)} {obj.GetType()} is not comparable to a {nameof(Rational)}", nameof(obj)),
                };
            }
            throw new ArgumentException($"{nameof(obj)} is not comparable to a {nameof(Rational)}", nameof(obj));
        }

        public static Rational Invert(in Rational value)
        {
            if (value.IsNaN || value.IsZero) return value;
            return new(-value.Numerator, value.Denominator);
        }
        public static Rational operator -(in Rational value) => Invert(value);
        public static Rational operator +(in Rational value) => value;

        public static Rational Add(in Rational left, in Rational right)
        {
            if (left.IsNaN || right.IsNaN) return NaN;
            if (left.IsInfinite) return right.IsFinite ? left : left.Sign != right.Sign ? Zero : left;
            if (right.IsInfinite) return right;

            var x = LowestCommonMultiple(left.Denominator, right.Denominator);
            var ay = x / left.Denominator * left.Numerator;
            var by = x / right.Denominator * right.Numerator;
            var y = ay + by;

            Reduce(ref y, ref x);
            return new(y, x);
        }
        public static Rational operator +(in Rational left, in Rational right) => Add(left, right);

        public static Rational Subtract(in Rational left, in Rational right)
        {
            if (left.IsNaN || right.IsNaN) return NaN;
            if (left.IsInfinite) return right.IsFinite ? left : left.Sign == right.Sign ? Zero : left;
            if (right.IsInfinite) return -right;

            var x = LowestCommonMultiple(left.Denominator, right.Denominator);
            var ay = x / left.Denominator * left.Numerator;
            var by = x / right.Denominator * right.Numerator;
            var y = ay - by;

            Reduce(ref y, ref x);

            return new(y, x);
        }
        public static Rational operator -(in Rational left, in Rational right) => Subtract(left, right);

        public static Rational Multiply(in Rational left, in Rational right)
        {
            var y = left.Numerator * right.Numerator;
            var x = left.Denominator * right.Denominator;

            Reduce(ref y, ref x);

            return new(y, x);
        }
        public static Rational operator *(in Rational left, in Rational right) => Multiply(left, right);

        public static Rational Divide(in Rational dividend, in Rational divisor)
        {
            if (dividend.IsInfinite && divisor.IsInfinite) return dividend.Sign * divisor.Sign;
            
            var y = dividend.Numerator * divisor.Denominator;
            var x = dividend.Denominator * divisor.Numerator;

            Reduce(ref y, ref x);

            return new(y, x);
        }
        public static Rational operator /(in Rational left, in Rational right) => Divide(left, right);

        public static Rational DivRem(in Rational dividend, in Rational divisor, out Rational remainder)
        {
            if (dividend.IsNaN || divisor.IsNaN) return remainder = NaN;
            remainder = Zero;
            if (divisor.IsInfinite)
            {
                if (dividend.IsInfinite) return dividend.Sign * divisor.Sign;
                remainder = dividend;
                return Zero;
            }
            if (divisor.IsZero) return dividend.IsZero ? NaN : dividend.Sign > 0 ? PositiveInfinity : NegativeInfinity;
            if (dividend.IsInfinite) return divisor.IsSubnormal ? -dividend : dividend;

            var x = LowestCommonMultiple(dividend.Denominator, divisor.Denominator);
            var ay = x / dividend.Denominator * dividend.Numerator;
            var by = x / divisor.Denominator * divisor.Numerator;

            var (ry, rx) = (ay % by, x);
            Reduce(ref ry, ref rx);
            remainder = new Rational(ry, rx);

            var y = ay / by;
            Reduce(ref y, ref x);

            return new(y, x);
        }

        public static Rational operator %(in Rational left, in Rational right)
        {
            DivRem(left, right, out var mod);
            return mod;
        }

        public static bool operator ==(Rational left, Rational right) =>
            left.Equals(right);

        public static bool operator !=(Rational left, Rational right) =>
            !left.Equals(right);

        public static bool operator <(Rational left, Rational right) =>
            left.CompareTo(right) < 0;

        public static bool operator <=(Rational left, Rational right) =>
            left.CompareTo(right) <= 0;

        public static bool operator >(Rational left, Rational right) =>
            left.CompareTo(right) > 0;

        public static bool operator >=(Rational left, Rational right) =>
            left.CompareTo(right) >= 0;
        #endregion
    }
}
