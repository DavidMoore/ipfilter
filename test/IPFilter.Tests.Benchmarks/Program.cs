using BenchmarkDotNet.Running;
using IPFilter.Tests.Benchmarks;

var summary = BenchmarkRunner.Run<InternetProtocolAddressBenchmarks>();