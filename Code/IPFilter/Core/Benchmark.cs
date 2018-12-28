namespace IPFilter.Core
{
    using System;
    using System.Diagnostics;

    /// <summary>
    ///     Helper class for timing an operation with a descriptive name. Traces
    ///     the start and completion, with elapsed time.
    /// </summary>
    /// <example>
    ///     <code>
    /// using(Benchmark.New("My operation name"))
    /// {
    ///     // Do stuff
    /// }
    /// </code>
    ///     Result:
    ///     <code>
    /// Starting [My operation name]
    /// Finished [My operation name] in 00:00:01.2002472
    /// </code>
    /// </example>
    class Benchmark : IDisposable
    {
        readonly string operation;
        readonly Stopwatch stopwatch;

        public static Benchmark New(string operation, params object[] args)
        {
            return New(string.Format(operation, args));
        }

        public static Benchmark New(string operation)
        {
            return new Benchmark(operation);
        }

        Benchmark(string operation)
        {
            this.operation = operation;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public void Dispose()
        {
            stopwatch.Stop();
            Trace.TraceInformation($"Finished [{operation}] in {stopwatch.Elapsed}");
            GC.SuppressFinalize(this);
        }
    }
}
