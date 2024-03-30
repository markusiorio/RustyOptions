using BenchmarkDotNet.Running;

namespace RustyOptions.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<FirstOrNoneBenchmarks>();
    }
}