using BenchmarkDotNet.Attributes;

namespace RustyOptions.Benchmarks;

public class LastOrNoneBenchmarks
{
    private readonly int[] intArray;
    private readonly List<int> intList;

    public LastOrNoneBenchmarks()
    {
        intArray = Enumerable.Range(1, 1000).ToArray();
        intList = [.. intArray];
    }

    [Benchmark]
    public void LastOrNoneEnumerable()
    {
        _ = IntEnumerable().LastOrNone();
    }

    [Benchmark]
    public void LastOrNoneArray()
    {
        _ = intArray.LastOrNone();
    }

    [Benchmark]
    public void LastOrNoneArrayOld()
    {
        _ = LastOrNoneOld(intArray);
    }

    [Benchmark]
    public void LastOrNoneList()
    {
        _ = intList.LastOrNone();
    }

    [Benchmark]
    public void LastOrNoneListOld()
    {
        _ = LastOrNoneOld(intList);
    }

    [Benchmark]
    public void LastOrNoneWithPredicateEnumerable()
    {
        _ = IntEnumerable().LastOrNone(x => x % 2 == 0);
    }

    [Benchmark]
    public void LastOrNoneWithPredicateArray()
    {
        _ = intArray.LastOrNone(x => x % 2 == 0);
    }

    [Benchmark]
    public void LastOrNoneWithPredicateList()
    {
        _ = intList.LastOrNone(x => x % 2 == 0);
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

    private IEnumerable<int> IntEnumerable()
    {
        for (int i = 0, len = intArray.Length; i < len; i++)
        {
            yield return intArray[i];
        }
    }
}