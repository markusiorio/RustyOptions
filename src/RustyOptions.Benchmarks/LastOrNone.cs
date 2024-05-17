using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace RustyOptions.Benchmarks;

[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[Config(typeof(Config))]
public class LastOrNoneBenchmarks
{
    const int SIZE = 1000;

    private readonly int[] intArray;
    private readonly List<int> intList;

    public LastOrNoneBenchmarks()
    {
        intArray = [.. Enumerable.Range(1, SIZE)];
        intList = [.. intArray];
    }

    [BenchmarkCategory("No-Predicate"), Benchmark]
    public void LastOrNoneEnumerable()
    {
        _ = IntEnumerable().LastOrNone();
    }

    [BenchmarkCategory("No-Predicate"), Benchmark]
    public void LastOrNoneArray()
    {
        _ = intArray.LastOrNone();
    }

    [BenchmarkCategory("No-Predicate"), Benchmark(Baseline = true)]
    public void LastOrNoneArrayOld()
    {
        _ = LastOrNoneOld(intArray);
    }

    [BenchmarkCategory("No-Predicate"), Benchmark]
    public void LastOrNoneList()
    {
        _ = intList.LastOrNone();
    }

    [BenchmarkCategory("No-Predicate"), Benchmark]
    public void LastOrNoneListOld()
    {
        _ = LastOrNoneOld(intList);
    }

    private static bool IsHalfway(int x) => x == 500;

    [BenchmarkCategory("Predicate"), Benchmark]
    public void LastOrNoneWithPredicateEnumerable()
    {
        _ = IntEnumerable().LastOrNone(IsHalfway);
    }

    [BenchmarkCategory("Predicate"), Benchmark]
    public void LastOrNoneWithPredicateArray()
    {
        _ = intArray.LastOrNone(IsHalfway);
    }

    [BenchmarkCategory("Predicate"), Benchmark(Baseline = true)]
    public void LastOrNoneWithPredicateArrayOld()
    {
        _ = LastOrNoneOld(intArray, IsHalfway);
    }

    [BenchmarkCategory("Predicate"), Benchmark]
    public void LastOrNoneWithPredicateList()
    {
        _ = intList.LastOrNone(IsHalfway);
    }

    [BenchmarkCategory("Predicate"), Benchmark]
    public void LastOrNoneWithPredicateListOld()
    {
        _ = LastOrNoneOld(intList, IsHalfway);
    }

    private static Option<T> LastOrNoneOld<T>(IEnumerable<T> items)
            where T : notnull
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items is IList<T> list)
        {
            return list.Count > 0 ? Option.Some(list[^1]) : default;
        }
        else if (items is IReadOnlyList<T> readOnlyList)
        {
            return readOnlyList.Count > 0 ? Option.Some(readOnlyList[^1]) : default;
        }
        else
        {
            using var enumerator = items.GetEnumerator();
            if (enumerator.MoveNext())
            {
                T result;
                do
                {
                    result = enumerator.Current;
                }
                while (enumerator.MoveNext());

                return Option.Some(result);
            }
        }

        return default;
    }

    public static Option<T> LastOrNoneOld<T>(IEnumerable<T> items, Func<T, bool> predicate)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(predicate);

        if (items is IList<T> list)
        {
            for (var i = list.Count - 1; i >= 0; --i)
            {
                var result = list[i];
                if (predicate(result))
                {
                    return Option.Some(result);
                }
            }

            return default;
        }
        else if (items is IReadOnlyList<T> readOnlyList)
        {
            for (var i = readOnlyList.Count - 1; i >= 0; --i)
            {
                var result = readOnlyList[i];
                if (predicate(result))
                {
                    return Option.Some(result);
                }
            }

            return default;
        }
        else
        {
            using var enumerator = items.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var result = enumerator.Current;
                if (predicate(result))
                {
                    while (enumerator.MoveNext())
                    {
                        var element = enumerator.Current;
                        if (predicate(element))
                        {
                            result = element;
                        }
                    }

                    return Option.Some(result);
                }
            }
        }

        return default;
    }

    private IEnumerable<int> IntEnumerable()
    {
        for (int i = 0, len = intArray.Length; i < len; i++)
        {
            yield return intArray[i];
        }
    }
}