using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Reports;

namespace RustyOptions.Benchmarks;

internal sealed class Config : ManualConfig
{
    public Config()
    {
        AddDiagnoser(MemoryDiagnoser.Default);
        SummaryStyle = SummaryStyle.Default.WithRatioStyle(RatioStyle.Percentage);
    }
}