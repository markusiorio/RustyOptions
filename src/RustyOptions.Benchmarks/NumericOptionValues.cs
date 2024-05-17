using BenchmarkDotNet.Attributes;

namespace RustyOptions.Benchmarks;

[Config(typeof(Config))]
public class NumericOptionValues
{
    const int SEED = 10007;
    const int SIZE = 1000;

    private readonly NumericOption<int>[] intArray;
    private readonly int[] valuesBuffer;

    public NumericOptionValues()
    {
        var rand = new Random(SEED);
        intArray = [.. Enumerable.Range(1, SIZE)
            .Select(_ => rand.NextDouble() < 0.45
                ? rand.Next().SomeNumeric() : NumericOption.None<int>())
        ];
        valuesBuffer = new int[intArray.Length];
    }

    [Benchmark(Baseline = true)]
    public void EnumerableValues()
    {
        int i = 0;
        foreach (var _ in intArray.AsEnumerable().Values())
        {
            i++;
        }
    }

    [Benchmark]
    public void ArrayValues()
    {
        int i = 0;
        int count = intArray.CopyValuesTo(valuesBuffer);
        foreach (var _ in valuesBuffer.AsSpan(0, count))
        {
            i++;
        }
    }
}