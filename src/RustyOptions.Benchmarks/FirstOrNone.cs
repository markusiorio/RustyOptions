using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace RustyOptions.Benchmarks;

[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[Config(typeof(Config))]
public class FirstOrNoneBenchmarks
{
    const int SIZE = 1000;

    private readonly int[] intArray;
    private readonly List<int> intList;

    public FirstOrNoneBenchmarks()
    {
        intArray = [.. Enumerable.Range(1, SIZE)];
        intList = [.. intArray];
    }

    [BenchmarkCategory("No-Predicate"), Benchmark]
    public void FirstOrNoneEnumerable()
    {
        _ = IntEnumerable().FirstOrNone();
    }

    [BenchmarkCategory("No-Predicate"), Benchmark]
    public void FirstOrNoneArray()
    {
        _ = intArray.FirstOrNone();
    }

    [BenchmarkCategory("No-Predicate"), Benchmark(Baseline = true)]
    public void FirstOrNoneArrayOld()
    {
        _ = FirstOrNoneOld(intArray);
    }

    [BenchmarkCategory("No-Predicate"), Benchmark]
    public void FirstOrNoneList()
    {
        _ = intList.FirstOrNone();
    }

    [BenchmarkCategory("No-Predicate"), Benchmark]
    public void FirstOrNoneListOld()
    {
        _ = FirstOrNoneOld(intList);
    }

    private static bool IsHalfway(int x) => x == SIZE / 2;

    [BenchmarkCategory("Predicate"), Benchmark]
    public void FirstOrNoneWithPredicateEnumerable()
    {
        _ = IntEnumerable().FirstOrNone(IsHalfway);
    }

    [BenchmarkCategory("Predicate"), Benchmark]
    public void FirstOrNoneWithPredicateArray()
    {
        _ = intArray.FirstOrNone(IsHalfway);
    }

    [BenchmarkCategory("Predicate"), Benchmark(Baseline = true)]
    public void FirstOrNoneWithPredicateArrayOld()
    {
        _ = FirstOrNoneOld(intArray, IsHalfway);
    }

    [BenchmarkCategory("Predicate"), Benchmark]
    public void FirstOrNoneWithPredicateList()
    {
        _ = intList.FirstOrNone(IsHalfway);
    }

    [BenchmarkCategory("Predicate"), Benchmark]
    public void FirstOrNoneWithPredicateListOld()
    {
        _ = FirstOrNoneOld(intList, IsHalfway);
    }

    private static Option<T> FirstOrNoneOld<T>(IEnumerable<T> items)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items is IList<T> list)
        {
            return list.Count > 0 ? Option.Some(list[0]) : default;
        }
        else if (items is IReadOnlyList<T> readOnlyList)
        {
            return readOnlyList.Count > 0 ? Option.Some(readOnlyList[0]) : default;
        }
        else
        {
            using var enumerator = items.GetEnumerator();
            if (enumerator.MoveNext())
            {
                return Option.Some(enumerator.Current);
            }
        }

        return default;
    }

    public static Option<T> FirstOrNoneOld<T>(IEnumerable<T> items, Func<T, bool> predicate)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(predicate);

        foreach (var item in items)
        {
            if (predicate(item))
            {
                return Option.Some(item);
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