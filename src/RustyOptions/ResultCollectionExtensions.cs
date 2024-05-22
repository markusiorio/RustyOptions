using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.ArgumentNullException;

namespace RustyOptions;

/// <summary>
/// Extension methods for working with collections involving <see cref="Result{T, TErr}"/>.
/// </summary>
public static class ResultCollectionExtensions
{
    /// <summary>
    /// Flattens a sequence of <see cref="Result{T, TErr}"/> into a sequence containing all inner values.
    /// Error results are discarded.
    /// </summary>
    /// <param name="self">The sequence of results.</param>
    /// <returns>A flattened sequence of values.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="self"/> is null.</exception>
    public static IEnumerable<T> Values<T, TErr>(this IEnumerable<Result<T, TErr>> self)
        where T : notnull
    {
        ThrowIfNull(self);

        foreach (var result in self)
        {
            if (result.IsOk(out var value))
            {
                yield return value;
            }
        }
    }

    /// <summary>
    /// Copies the inner values of all <see cref="Result{T, TErr}"/> in an array 
    /// to a <paramref name="destination"/> span, returning the destination span 
    /// trimmed to the number of values copied. Error results are discarded.
    /// </summary>
    /// <param name="self">The source span of results.</param>
    /// <param name="destination">The destination span to copy the values to.</param>
    /// <returns>A flattened sequence of values.</returns>
    /// <exception cref="System.IndexOutOfRangeException">
    /// Thrown if the destination span is too small to hold all the values.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> CopyValuesTo<T, TErr>(this Result<T, TErr>[] self, Span<T> destination)
        where T : notnull
    {
        return CopyValuesTo((ReadOnlySpan<Result<T, TErr>>)self, destination);
    }

    /// <summary>
    /// Copies the inner values of all <see cref="Result{T, TErr}"/> in a List 
    /// to a <paramref name="destination"/> span, returning the destination span 
    /// trimmed to the number of values copied. Error results are discarded.
    /// </summary>
    /// <param name="self">The source span of results.</param>
    /// <param name="destination">The destination span to copy the values to.</param>
    /// <returns>A flattened sequence of values.</returns>
    /// <exception cref="System.IndexOutOfRangeException">
    /// Thrown if the destination span is too small to hold all the values.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> CopyValuesTo<T, TErr>(this List<Result<T, TErr>> self, Span<T> destination)
        where T : notnull
    {
        return CollectionsMarshal.AsSpan(self).CopyValuesTo(destination);
    }

    /// <summary>
    /// Copies the inner values of all <see cref="Result{T, TErr}"/> in a span 
    /// to a <paramref name="destination"/> span, returning the destination span 
    /// trimmed to the number of values copied. Error results are discarded.
    /// </summary>
    /// <param name="self">The source span of results.</param>
    /// <param name="destination">The destination span to copy the values to.</param>
    /// <returns>A flattened sequence of values.</returns>
    /// <exception cref="System.IndexOutOfRangeException">
    /// Thrown if the destination span is too small to hold all the values.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> CopyValuesTo<T, TErr>(this Span<Result<T, TErr>> self, Span<T> destination)
        where T : notnull
    {
        return CopyValuesTo((ReadOnlySpan<Result<T, TErr>>)self, destination);
    }

    /// <summary>
    /// Copies the inner values of all <see cref="Result{T, TErr}"/> in a span 
    /// to a <paramref name="destination"/> span, returning the destination span 
    /// trimmed to the number of values copied. Error results are discarded.
    /// </summary>
    /// <param name="self">The source span of results.</param>
    /// <param name="destination">The destination span to copy the values to.</param>
    /// <returns>A flattened sequence of values.</returns>
    /// <exception cref="System.IndexOutOfRangeException">
    /// Thrown if the destination span is too small to hold all the values.
    /// </exception>
    public static Span<T> CopyValuesTo<T, TErr>(this ReadOnlySpan<Result<T, TErr>> self, Span<T> destination)
        where T : notnull
    {
        int j = 0;
        for (int i = 0; i < self.Length; i++)
        {
            if (self[i].IsOk(out var value))
            {
                destination[j++] = value;
            }
        }
        return destination[..j];
    }

    /// <summary>
    /// Flattens a sequence of <see cref="Result{T, TErr}"/> into a sequence containing all error values.
    /// Ok results are discarded.
    /// </summary>
    /// <param name="self">The sequence of results.</param>
    /// <returns>A flattened sequence of error values.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="self"/> is null.</exception>
    public static IEnumerable<TErr> Errors<T, TErr>(this IEnumerable<Result<T, TErr>> self)
        where T : notnull
    {
        ThrowIfNull(self);

        foreach (var result in self)
        {
            if (result.IsErr(out var err) && err is not null)
            {
                yield return err;
            }
        }
    }

    /// <summary>
    /// Copies the error values of all <see cref="Result{T, TErr}"/> in an array 
    /// to a <paramref name="destination"/> span, returning the destination span 
    /// trimmed to the number of values copied. Error results are discarded.
    /// </summary>
    /// <param name="self">The source span of results.</param>
    /// <param name="destination">The destination span to copy the errors to.</param>
    /// <returns>A flattened sequence of values.</returns>
    /// <exception cref="System.IndexOutOfRangeException">
    /// Thrown if the destination span is too small to hold all the errors.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<TErr> CopyErrorssTo<T, TErr>(this Result<T, TErr>[] self, Span<TErr> destination)
        where T : notnull
    {
        return CopyErrorsTo((ReadOnlySpan<Result<T, TErr>>)self, destination);
    }

    /// <summary>
    /// Copies the error values of all <see cref="Result{T, TErr}"/> in a List 
    /// to a <paramref name="destination"/> span, returning the destination span 
    /// trimmed to the number of values copied. Error results are discarded.
    /// </summary>
    /// <param name="self">The source span of results.</param>
    /// <param name="destination">The destination span to copy the errors to.</param>
    /// <returns>A flattened sequence of values.</returns>
    /// <exception cref="System.IndexOutOfRangeException">
    /// Thrown if the destination span is too small to hold all the errors.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<TErr> CopyErrorsTo<T, TErr>(this List<Result<T, TErr>> self, Span<TErr> destination)
        where T : notnull
    {
        return CollectionsMarshal.AsSpan(self).CopyErrorsTo(destination);
    }

    /// <summary>
    /// Copies the error values of all <see cref="Result{T, TErr}"/> in a span 
    /// to a <paramref name="destination"/> span, returning the destination span 
    /// trimmed to the number of values copied. Error results are discarded.
    /// </summary>
    /// <param name="self">The source span of results.</param>
    /// <param name="destination">The destination span to copy the errors to.</param>
    /// <returns>A flattened sequence of values.</returns>
    /// <exception cref="System.IndexOutOfRangeException">
    /// Thrown if the destination span is too small to hold all the errors.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<TErr> CopyErrorsTo<T, TErr>(this Span<Result<T, TErr>> self, Span<TErr> destination)
        where T : notnull
    {
        return CopyErrorsTo((ReadOnlySpan<Result<T, TErr>>)self, destination);
    }

    /// <summary>
    /// Copies the error values of all <see cref="Result{T, TErr}"/> in a span 
    /// to a <paramref name="destination"/> span, returning the destination span 
    /// trimmed to the number of values copied. Error results are discarded.
    /// </summary>
    /// <param name="self">The source span of results.</param>
    /// <param name="destination">The destination span to copy the errors to.</param>
    /// <returns>A flattened sequence of values.</returns>
    /// <exception cref="System.IndexOutOfRangeException">
    /// Thrown if the destination span is too small to hold all the errors.
    /// </exception>
    public static Span<TErr> CopyErrorsTo<T, TErr>(this ReadOnlySpan<Result<T, TErr>> self, Span<TErr> destination)
        where T : notnull
    {
        int j = 0;
        for (int i = 0; i < self.Length; i++)
        {
            if (self[i].IsErr(out var err) && err is not null)
            {
                destination[j++] = err;
            }
        }
        return destination[..j];
    }
}
