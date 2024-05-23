﻿#if NET7_0_OR_GREATER

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
#if NET8_0_OR_GREATER
using System.Text.Unicode;
#endif

using static System.ArgumentNullException;

namespace RustyOptions;

/// <summary>
/// <see cref="NumericOption{T}"/> represents an optional number: every 
/// <see cref="NumericOption{T}"/> is either 'Some' and contains a number, or 
/// 'None', and does not. 
/// </summary>
/// <typeparam name="T">The type the option might contain.</typeparam>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Required for INumber.")]
[Serializable]
[JsonConverter(typeof(NumericOptionJsonConverter))]
public readonly struct NumericOption<T> : 
    IEquatable<NumericOption<T>>, IComparable<NumericOption<T>>, INumber<NumericOption<T>>,
#if NET8_0_OR_GREATER
    IUtf8SpanFormattable,
#endif
    IFormattable, ISpanFormattable
    where T : struct, INumber<T>
{
    /// <summary>
    /// Returns the <c>None</c> option for the specified <typeparamref name="T"/>.
    /// </summary>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "The syntax `NumericOption<T>.None` is too nice to give up.")]
    public static NumericOption<T> None
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => default;
    }

    /// <inheritdoc/>
    public static NumericOption<T> Zero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(T.Zero);
    }

    /// <inheritdoc/>
    public static NumericOption<T> One
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(T.One);
    }

    static int INumberBase<NumericOption<T>>.Radix => T.Radix;

    static NumericOption<T> IAdditiveIdentity<NumericOption<T>, NumericOption<T>>.AdditiveIdentity
        => new(T.AdditiveIdentity);

    static NumericOption<T> IMultiplicativeIdentity<NumericOption<T>, NumericOption<T>>.MultiplicativeIdentity
        => new(T.MultiplicativeIdentity);

    private readonly bool _isSome;
    private readonly T _value;

    /// <summary>
    /// Creates an <see cref="Option{T}"/> containing the given value.
    /// <para>NOTE: Nulls are not allowed; a null value will result in a <c>None</c> option.</para>
    /// </summary>
    /// <param name="value">The value to wrap in an <see cref="Option{T}"/>.</param>
    public NumericOption(T value)
    {
        _value = value;
        _isSome = true;
    }

    /// <summary>
    /// Returns <c>true</c> if the option is <c>None</c>.
    /// </summary>
    public bool IsNone => !_isSome;

    /// <summary>
    /// Returns <c>true</c> if the option is <c>Some</c>, and returns the contained
    /// value through <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value contained in the option.</param>
    /// <returns><c>true</c> if the option is <c>Some</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSome([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return _isSome;
    }

    /// <summary>
    /// Returns the result of executing the <paramref name="onSome"/>
    /// or <paramref name="onNone"/> functions, depending on the state 
    /// of the <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T2">The return type of the given functions.</typeparam>
    /// <param name="onSome">The function to pass the value to, if the result is <c>Ok</c>.</param>
    /// <param name="onNone">The function to pass the error value to, if the result is <c>Err</c>.</param>
    /// <returns>The value returned by the called function.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if either <paramref name="onSome"/> or <paramref name="onNone"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T2 Match<T2>(Func<T, T2> onSome, Func<T2> onNone)
    {
        ThrowIfNull(onSome);
        ThrowIfNull(onNone);
        return _isSome ? onSome(_value) : onNone();
    }

    /// <summary>
    /// If the option is <c>Some</c>, passes the contained value to the <paramref name="onSome"/> function.
    /// Otherwise calls the <paramref name="onNone"/> function.
    /// </summary>
    /// <param name="onSome">The function to call with the contained <c>Some</c> value, if any.</param>
    /// <param name="onNone">The function to call if the option is <c>None</c>.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if either <paramref name="onSome"/> or <paramref name="onNone"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> onSome, Action onNone)
    {
        ThrowIfNull(onSome);
        ThrowIfNull(onNone);
        if (_isSome)
            onSome(_value);
        else
            onNone();
    }

    /// <summary>
    /// Returns the contained <c>Some</c> value, or throws an <see cref="InvalidOperationException"/>
    /// if the value is <c>None</c>.
    /// </summary>
    /// <returns>The value contained in the option.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the option does not contain a value.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap()
    {
        return _isSome ? _value
            : throw new InvalidOperationException("The option was expected to contain a value, but did not.");
    }

    /// <summary>
    /// Converts the option into a <see cref="ReadOnlySpan{T}"/> that contains either zero or one
    /// items depending on whether the option is <c>None</c> or <c>Some</c>.
    /// </summary>
    /// <returns>A span containing the option's value, or an empty span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsSpan()
    {
        return _isSome
            ? MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in _value), 1)
            : [];
    }

    /// <summary>
    /// Returns an <see cref="IEnumerable{T}"/> containing either zero or one value,
    /// depending on whether the option is <c>None</c> or <c>Some</c>.
    /// </summary>
    /// <returns>An enumerable containing the option's value, or an empty enumerable.</returns>
    public IEnumerable<T> AsEnumerable()
    {
        if (_isSome)
        {
            yield return _value;
        }
    }

    /// <summary>
    /// Returns an enumerator for the option.
    /// </summary>
    /// <returns>The enumerator.</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IEnumerator<T> GetEnumerator()
    {
        if (_isSome)
        {
            yield return _value;
        }
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(NumericOption<T> other)
    {
        if (_isSome != other._isSome)
            return false;

        if (!_isSome)
            return true;

        return EqualityComparer<T>.Default.Equals(_value, other._value);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object.
    /// </summary>
    /// <param name="obj">An object to compare with this object.</param>
    /// <returns>
    /// <c>true</c> if the current object is equal to the <paramref name="obj"/> parameter;
    /// otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is NumericOption<T> opt && Equals(opt);

    /// <summary>
    /// Retrieves the hash code of the object contained by the <see cref="Option{T}"/>, if any.
    /// </summary>
    /// <returns>
    /// The hash code of the object returned by the <see cref="IsSome(out T)"/> method, if that
    /// method returns <c>true</c>, or zero if it returns <c>false</c>.
    /// </returns>
    public override int GetHashCode()
        => _isSome ? _value.GetHashCode() : 0;

    /// <summary>
    /// Returns the text representation of the value of the current <see cref="Option{T}"/> object.
    /// </summary>
    /// <returns>
    /// The text representation of the value of the current <see cref="Option{T}"/> object.
    /// </returns>
    public override string ToString()
    {
        return _isSome ? $"Some({_value})" : "None";
    }

    /// <summary>
    /// Formats the value of the current <see cref="Option{T}"/> using the specified format.
    /// </summary>
    /// <param name="format">
    /// The format to use, or a null reference to use the default format defined for
    /// the type of the contained value.
    /// </param>
    /// <param name="formatProvider">
    /// The provider to use to format the value, or a null reference to obtain the
    /// format information from the current locale setting of the operating system.
    /// </param>
    /// <returns>The value of the current instance in the specified format.</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format))
        {
            return _isSome
                ? string.Create(formatProvider, $"Some({_value})")
                : "None";
        }

        return _isSome
            ? string.Format(formatProvider, "Some({0:" + format + "})", _value)
            : "None";
    }

    /// <summary>
    /// Tries to format the value of the current instance into the provided span of characters.
    /// </summary>
    /// <param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    /// <param name="charsWritten">When this method returns, contains the number of characters that were written in destination.</param>
    /// <param name="format">
    /// A span containing the characters that represent a standard or custom format string that defines the acceptable format for destination.
    /// </param>
    /// <param name="provider">An optional object that supplies culture-specific formatting information for destination.</param>
    /// <returns><c>true</c> if the formatting was successful; otherwise, <c>false</c>.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (_isSome)
        {
            if ("Some(".TryCopyTo(destination) && _value.TryFormat(destination[5..], out var innerWritten, format, provider))
            {
                destination = destination[(5 + innerWritten)..];
                if (destination.Length >= 1)
                {
                    destination[0] = ')';
                    charsWritten = innerWritten + 6;
                    return true;
                }
            }

            charsWritten = 0;
            return false;
        }
        else
        {
            if ("None".TryCopyTo(destination))
            {
                charsWritten = 4;
                return true;
            }
        }

        charsWritten = 0;
        return false;
    }

#if NET8_0_OR_GREATER
    /// <inheritdoc/>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (_isSome)
        {
            if (_value is IUtf8SpanFormattable spanFormattable)
            {
                if ("Some("u8.TryCopyTo(utf8Destination) && spanFormattable.TryFormat(utf8Destination[5..], out int innerWritten, format, provider))
                {
                    utf8Destination = utf8Destination[(innerWritten + 5)..];
                    if (!utf8Destination.IsEmpty)
                    {
                        utf8Destination[0] = (byte)')';
                        bytesWritten = innerWritten + 6;
                        return true;
                    }
                }

                bytesWritten = 0;
                return false;
            }
            else
            {
                var output = this.ToString(format.IsEmpty ? null : format.ToString(), provider);

                if (output is not null && utf8Destination.Length >= output.Length)
                {
                    Utf8.FromUtf16(output, utf8Destination, out _, out bytesWritten);
                    return true;
                }
            }
        }
        else
        {
            if ("None"u8.TryCopyTo(utf8Destination))
            {
                bytesWritten = 4;
                return true;
            }
        }

        bytesWritten = 0;
        return false;
    }
#endif

    /// <summary>
    /// Compares the current instance with another object of the same type and returns an integer
    /// that indicates whether the current instance precedes, follows, or occurs in the same
    /// position in the sort order as the other object.
    /// <para><c>Some</c> compares as less than any <c>None</c>, while two <c>Some</c> compare as their contained values would in <typeparamref name="T"/>.</para>
    /// </summary>
    /// <param name="other"></param>
    /// <returns>
    /// <c>-1</c> if this instance precendes <paramref name="other"/>, <c>0</c> if they are equal, and <c>1</c> if this instance follows <paramref name="other"/>.
    /// </returns>
    public int CompareTo(NumericOption<T> other)
    {
        return (_isSome, other._isSome) switch
        {
            (true, true) => Comparer<T>.Default.Compare(_value, other._value),
            (true, false) => -1,
            (false, true) => 1,
            (false, false) => 0
        };
    }

    /// <inheritdoc/>
    public int CompareTo(object? obj) => obj is NumericOption<T> opt ? CompareTo(opt) : -1;

    /// <inheritdoc/>
    public static NumericOption<T> Abs(NumericOption<T> value)
        => value.Map(T.Abs);

    /// <inheritdoc/>
    public static NumericOption<T> Max(NumericOption<T> x, NumericOption<T> y)
    {
        return (x._isSome, y._isSome) switch
        {
            (true, true) => new(T.Max(x._value, y._value)),
            (false, true) => y,
            (true, false) => x,
            (false, false) => default
        };
    }

    /// <inheritdoc/>
    public static NumericOption<T> Min(NumericOption<T> x, NumericOption<T> y)
    {
        return (x._isSome, y._isSome) switch
        {
            (true, true) => new(T.Min(x._value, y._value)),
            (false, true) => y,
            (true, false) => x,
            (false, false) => default
        };
    }

    /// <inheritdoc/>
    public static NumericOption<T> Clamp(NumericOption<T> value, NumericOption<T> min, NumericOption<T> max)
    {
        return value._isSome && min._isSome && max._isSome
            ? new(T.Clamp(value._value, min._value, max._value)) : default;
    }

    static bool INumberBase<NumericOption<T>>.IsCanonical(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsCanonical(x);

    static bool INumberBase<NumericOption<T>>.IsComplexNumber(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsComplexNumber(x);

    /// <inheritdoc/>
    public static bool IsEvenInteger(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsEvenInteger(x);

    static bool INumberBase<NumericOption<T>>.IsFinite(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsFinite(x);

    static bool INumberBase<NumericOption<T>>.IsImaginaryNumber(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsImaginaryNumber(x);

    /// <inheritdoc/>
    public static bool IsInfinity(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsInfinity(x);

    static bool INumberBase<NumericOption<T>>.IsInteger(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsInteger(x);

    /// <inheritdoc/>
    public static bool IsNaN(NumericOption<T> value)
        => !value.IsSome(out var x) || T.IsNaN(x);

    /// <inheritdoc/>
    public static bool IsNegative(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsNegative(x);

    /// <inheritdoc/>
    public static bool IsNegativeInfinity(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsNegativeInfinity(x);

    static bool INumberBase<NumericOption<T>>.IsNormal(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsNormal(x);

    /// <inheritdoc/>
    public static bool IsOddInteger(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsOddInteger(x);

    /// <inheritdoc/>
    public static bool IsPositive(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsPositive(x);

    /// <inheritdoc/>
    public static bool IsPositiveInfinity(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsPositiveInfinity(x);

    static bool INumberBase<NumericOption<T>>.IsRealNumber(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsRealNumber(x);

    static bool INumberBase<NumericOption<T>>.IsSubnormal(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsSubnormal(x);

    /// <inheritdoc/>
    public static bool IsZero(NumericOption<T> value)
        => value.IsSome(out var x) && T.IsZero(x);

    static NumericOption<T> INumberBase<NumericOption<T>>.MaxMagnitude(NumericOption<T> x, NumericOption<T> y)
        => x._isSome && y._isSome ? new(T.MaxMagnitude(x._value, y._value)) : default;

    static NumericOption<T> INumberBase<NumericOption<T>>.MaxMagnitudeNumber(NumericOption<T> x, NumericOption<T> y)
        => x._isSome && y._isSome ? new(T.MaxMagnitudeNumber(x._value, y._value)) : default;

    static NumericOption<T> INumberBase<NumericOption<T>>.MinMagnitude(NumericOption<T> x, NumericOption<T> y)
        => x._isSome && y._isSome ? new(T.MinMagnitude(x._value, y._value)) : default;

    static NumericOption<T> INumberBase<NumericOption<T>>.MinMagnitudeNumber(NumericOption<T> x, NumericOption<T> y)
        => x._isSome && y._isSome ? new(T.MinMagnitudeNumber(x._value, y._value)) : default;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumericOption<T> Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        return T.TryParse(s, style, provider, out var parsed)
            ? new(parsed) : default;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumericOption<T> Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        return T.TryParse(s, style, provider, out var parsed)
            ? new(parsed) : default;
    }

    static bool INumberBase<NumericOption<T>>.TryParse(ReadOnlySpan<char> s,
                                                       NumberStyles style,
                                                       IFormatProvider? provider,
                                                       [MaybeNullWhen(false)] out NumericOption<T> result)
    {
        if (T.TryParse(s, style, provider, out var parsed))
        {
            result = new(parsed);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc/>
    static bool INumberBase<NumericOption<T>>.TryParse([NotNullWhen(true)] string? s,
                                                       NumberStyles style,
                                                       IFormatProvider? provider,
                                                       [MaybeNullWhen(false)] out NumericOption<T> result)
    {
        if (T.TryParse(s, style, provider, out var parsed))
        {
            result = new(parsed);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumericOption<T> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return T.TryParse(s, provider, out var parsed)
            ? new(parsed) : default;
    }

    static bool ISpanParsable<NumericOption<T>>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider,
                                                         [MaybeNullWhen(false)] out NumericOption<T> result)
    {
        if (T.TryParse(s, provider, out var parsed))
        {
            result = new(parsed);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumericOption<T> Parse(string s, IFormatProvider? provider)
    {
        return T.TryParse(s, provider, out var parsed)
            ? new(parsed) : default;
    }

    static bool IParsable<NumericOption<T>>.TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider,
                                                     [MaybeNullWhen(false)] out NumericOption<T> result)
    {
        if (T.TryParse(s, provider, out var parsed))
        {
            result = new(parsed);
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc/>
    public static NumericOption<T> CreateChecked<TOther>(TOther value)
        where TOther : INumberBase<TOther>
    {
        NumericOption<T> result;

        if (typeof(TOther) == typeof(NumericOption<T>))
        {
            result = (NumericOption<T>)(object)value;
        }
        else if (!TryConvertFromChecked(value, out result) && !TOther.TryConvertToChecked(value, out result))
        {
            result = default;
        }

        return result;
    }

    /// <inheritdoc/>
    public static NumericOption<T> CreateSaturating<TOther>(TOther value)
        where TOther : INumberBase<TOther>
    {
        NumericOption<T> result;

        if (typeof(TOther) == typeof(NumericOption<T>))
        {
            result = (NumericOption<T>)(object)value;
        }
        else if (!TryConvertFromSaturating(value, out result) && !TOther.TryConvertToSaturating(value, out result))
        {
            result = default;
        }

        return result;
    }

    /// <inheritdoc/>
    public static NumericOption<T> CreateTruncating<TOther>(TOther value)
        where TOther : INumberBase<TOther>
    {
        NumericOption<T> result;

        if (typeof(TOther) == typeof(NumericOption<T>))
        {
            result = (NumericOption<T>)(object)value;
        }
        else if (!TryConvertFromTruncating(value, out result) && !TOther.TryConvertToTruncating(value, out result))
        {
            result = default;
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<NumericOption<T>>.TryConvertFromChecked<TOther>(TOther value, out NumericOption<T> result)
        => TryConvertFromChecked(value, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertFromChecked<TOther>(TOther value, out NumericOption<T> result)
        where TOther : INumberBase<TOther>
    {
        if (T.TryConvertFromChecked(value, out var converted))
        {
            result = new(converted);
            return true;
        }

        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<NumericOption<T>>.TryConvertFromSaturating<TOther>(TOther value, out NumericOption<T> result)
        => TryConvertFromSaturating(value, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertFromSaturating<TOther>(TOther value, out NumericOption<T> result)
        where TOther : INumberBase<TOther>
    {
        if (T.TryConvertFromSaturating(value, out var converted))
        {
            result = new(converted);
            return true;
        }

        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<NumericOption<T>>.TryConvertFromTruncating<TOther>(TOther value, out NumericOption<T> result)
        => TryConvertFromTruncating(value, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertFromTruncating<TOther>(TOther value, out NumericOption<T> result)
        where TOther : INumberBase<TOther>
    {
        if (T.TryConvertFromTruncating(value, out var converted))
        {
            result = new(converted);
            return true;
        }

        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<NumericOption<T>>.TryConvertToChecked<TOther>(NumericOption<T> value, [MaybeNullWhen(false)] out TOther result)
        => TryConvertToChecked(value, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertToChecked<TOther>(NumericOption<T> value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther>
    {
        if (value.IsSome(out var x) && T.TryConvertToChecked<TOther>(x, out var converted))
        {
            result = converted;
            return true;
        }

        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<NumericOption<T>>.TryConvertToSaturating<TOther>(NumericOption<T> value, [MaybeNullWhen(false)] out TOther result)
        => TryConvertToSaturating(value, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertToSaturating<TOther>(NumericOption<T> value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther>
    {
        if (value.IsSome(out var x) && T.TryConvertToSaturating<TOther>(x, out var converted))
        {
            result = converted;
            return true;
        }

        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool INumberBase<NumericOption<T>>.TryConvertToTruncating<TOther>(NumericOption<T> value, [MaybeNullWhen(false)] out TOther result)
        => TryConvertToTruncating(value, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertToTruncating<TOther>(NumericOption<T> value, [MaybeNullWhen(false)] out TOther result)
        where TOther : INumberBase<TOther>
    {
        if (value.IsSome(out var x) && T.TryConvertToTruncating<TOther>(x, out var converted))
        {
            result = converted;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Determines whether one <c>Option</c> is equal to another <c>Option</c>.
    /// </summary>
    /// <param name="left">The first <c>Option</c> to compare.</param>
    /// <param name="right">The second <c>Option</c> to compare.</param>
    /// <returns><c>true</c> if the two values are equal.</returns>
    public static bool operator ==(NumericOption<T> left, NumericOption<T> right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether one <c>Option</c> is not equal to another <c>Option</c>.
    /// </summary>
    /// <param name="left">The first <c>Option</c> to compare.</param>
    /// <param name="right">The second <c>Option</c> to compare.</param>
    /// <returns><c>true</c> if the two values are not equal.</returns>
    public static bool operator !=(NumericOption<T> left, NumericOption<T> right)
        => !left.Equals(right);

    /// <summary>
    /// Determines whether one <c>Option</c> is greater than another <c>Option</c>.
    /// </summary>
    /// <param name="left">The first <c>Option</c> to compare.</param>
    /// <param name="right">The second <c>Option</c> to compare.</param>
    /// <returns><c>true</c> if the <paramref name="left"/> parameter is greater than the <paramref name="right"/> parameter.</returns>
    public static bool operator >(NumericOption<T> left, NumericOption<T> right)
        => left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether one <c>Option</c> is less than another <c>Option</c>.
    /// </summary>
    /// <param name="left">The first <c>Option</c> to compare.</param>
    /// <param name="right">The second <c>Option</c> to compare.</param>
    /// <returns><c>true</c> if the <paramref name="left"/> parameter is less than the <paramref name="right"/> parameter.</returns>
    public static bool operator <(NumericOption<T> left, NumericOption<T> right)
        => left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether one <c>Option</c> is greater than or equal to another <c>Option</c>.
    /// </summary>
    /// <param name="left">The first <c>Option</c> to compare.</param>
    /// <param name="right">The second <c>Option</c> to compare.</param>
    /// <returns><c>true</c> if the <paramref name="left"/> parameter is greater than or equal to the <paramref name="right"/> parameter.</returns>
    public static bool operator >=(NumericOption<T> left, NumericOption<T> right)
        => left.CompareTo(right) >= 0;

    /// <summary>
    /// Determines whether one <c>Option</c> is less than or equal to another <c>Option</c>.
    /// </summary>
    /// <param name="left">The first <c>Option</c> to compare.</param>
    /// <param name="right">The second <c>Option</c> to compare.</param>
    /// <returns><c>true</c> if the <paramref name="left"/> parameter is less than or equal to the <paramref name="right"/> parameter.</returns>
    public static bool operator <=(NumericOption<T> left, NumericOption<T> right)
        => left.CompareTo(right) <= 0;

    /// <summary>
    /// Converts a <see cref="NumericOption{T}"/> into an <see cref="Option{T}"/>.
    /// </summary>
    /// <param name="self">The numeric option to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Option<T>(NumericOption<T> self)
        => self.IsSome(out var val) ? Option.Some(val) : Option<T>.None;

    /// <summary>
    /// Implicitly converts a <typeparamref name="T"/> into a <see cref="NumericOption{T}"/>.
    /// </summary>
    /// <param name="value">The value to wrap in a <see cref="NumericOption{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator NumericOption<T>(T value) => new(value);

    /// <summary>
    /// Implicitly converts <see cref="Option{T}"/> into <see cref="NumericOption{T}"/>, provided that <c>T</c> is compatible with NumericOption.
    /// </summary>
    /// <param name="option">The option to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator NumericOption<T>(Option<T> option)
        => option.IsSome(out var val) ? new(val) : default;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumericOption<T> operator +(NumericOption<T> left, NumericOption<T> right)
        => left._isSome && right._isSome ? new(left._value + right._value) : default;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumericOption<T> operator -(NumericOption<T> left, NumericOption<T> right)
        => left._isSome && right._isSome ? new(left._value - right._value) : default;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumericOption<T> operator *(NumericOption<T> left, NumericOption<T> right)
        => left._isSome && right._isSome ? new(left._value * right._value) : default;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumericOption<T> operator /(NumericOption<T> left, NumericOption<T> right)
        => left._isSome && right._isSome ? new(left._value / right._value) : default;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumericOption<T> operator ++(NumericOption<T> value)
        => value._isSome ? new(value._value + T.One) : default;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumericOption<T> operator --(NumericOption<T> value)
        => value._isSome ? new(value._value - T.One) : default;

    /// <inheritdoc/>
    public static NumericOption<T> operator +(NumericOption<T> value)
        => value._isSome ? new(+value._value) : default;

    /// <inheritdoc/>
    public static NumericOption<T> operator -(NumericOption<T> value)
        => value._isSome ? new(-value._value) : default;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumericOption<T> operator %(NumericOption<T> left, NumericOption<T> right)
        => left._isSome && right._isSome ? new(left._value % right._value) : default;
}

#endif