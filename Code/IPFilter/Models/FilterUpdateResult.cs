namespace IPFilter.Models
{
    using System;

    public class FilterUpdateResult
    {
        public TimeSpan? UpdateTime { get; set; }

        public DateTimeOffset? FilterTimestamp { get; set; }
    }
}