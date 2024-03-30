using BenchmarkDotNet.Attributes;
using RustyOptions;

namespace RustyOptions.Benchmarks;

public class FirstOrNoneBenchmarks
{
    private readonly int[] intArray;
    private readonly List<int> intList;

    public FirstOrNoneBenchmarks()
    {
        intArray = Enumerable.Range(1, 1000).ToArray();
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

    [Benchmark]
    public void FirstOrNoneList()
    {
        _ = intList.FirstOrNone();
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

    private IEnumerable<int> IntEnumerable()
    {
        for (int i = 0, len = intArray.Length; i < len; i++)
        {
            yield return intArray[i];
        }
    }
}