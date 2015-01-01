namespace IPFilter
{
    using System;

    class FilterUpdateResult
    {
        public TimeSpan? UpdateTime { get; set; }

        public DateTimeOffset? FilterTimestamp { get; set; }
    }
}