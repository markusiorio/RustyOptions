using BenchmarkDotNet.Attributes;

namespace RustyOptions.Benchmarks;

public class FirstOrNoneBenchmarks
{
    private readonly int[] intArray;
    private readonly List<int> intList;

    public FirstOrNoneBenchmarks()
    {
        intArray = [.. Enumerable.Range(1, 1000)];
        intList = [.. intArray];
    }

    [Benchmark]
    public void FirstOrNoneEnumerable()
    {
        _ = IntEnumerable().FirstOrNone();
    }

    [Benchmark]
    public void FirstOrNoneArray()
    {
        _ = intArray.FirstOrNone();
    }

    [Benchmark(Baseline = true)]
    public void FirstOrNoneArrayOld()
    {
        _ = FirstOrNoneOld(intArray);
    }

    [Benchmark]
    public void FirstOrNoneList()
    {
        _ = intList.FirstOrNone();
    }

    [Benchmark]
    public void FirstOrNoneListOld()
    {
        _ = FirstOrNoneOld(intList);
    }

    [Benchmark]
    public void FirstOrNoneWithPredicateEnumerable()
    {
        _ = IntEnumerable().FirstOrNone(x => x % 2 == 0);
    }

    [Benchmark]
    public void FirstOrNoneWithPredicateArray()
    {
        _ = intArray.FirstOrNone(x => x % 2 == 0);
    }

    [Benchmark]
    public void FirstOrNoneWithPredicateList()
    {
        _ = intList.FirstOrNone(x => x % 2 == 0);
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

    private IEnumerable<int> IntEnumerable()
    {
        for (int i = 0, len = intArray.Length; i < len; i++)
        {
            yield return intArray[i];
        }
    }
}