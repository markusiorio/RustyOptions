using static RustyOptions.Result;

namespace RustyOptions.Tests;

public class ResultCollectionTests
{
    [Fact]
    public void CanGetValues()
    {
        var results = Enumerable.Range(1, 10)
            .Select(x => (x & 1) == 0 ? Ok(x) : Err<int>("odd"));

        Assert.Equal([2, 4, 6, 8, 10], [.. results.Values()]);
    }

    [Fact]
    public void CanGetErrors()
    {
        var results = Enumerable.Range(1, 10)
            .Select(x => (x & 1) == 0 ? Ok(x) : Err<int>("odd"));

        Assert.Equal(Enumerable.Repeat("odd", 5), results.Errors());
    }
}

