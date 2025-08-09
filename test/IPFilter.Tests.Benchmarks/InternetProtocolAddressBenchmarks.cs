using BenchmarkDotNet.Attributes;
using IPFilter.Core;

namespace IPFilter.Tests.Benchmarks;

[MemoryDiagnoser]
public class InternetProtocolAddressBenchmarks
{
    readonly List<string> addresses = [];

    public InternetProtocolAddressBenchmarks()
    {
        // Build a list of random IP addresses
        var random = new Random(42);
        for (var i = 0; i < 1_000_000; i++)
        {
            var octets = new byte[4];
            random.NextBytes(octets);
            addresses.Add($"{octets[0]}.{octets[1]}.{octets[2]}.{octets[3]}");
        }
    }

    [Benchmark]
    public void Parse()
    {
        foreach (var address in addresses)
        {
            InternetAddress.Parse(address);
        }
    }
}