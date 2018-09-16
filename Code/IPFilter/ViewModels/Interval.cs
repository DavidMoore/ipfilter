namespace IPFilter.ViewModels
{
    using System;

    public class Interval
    {
        /// <summary>Initializes a new instance of the <see cref="Interval" /> class.</summary>
        public Interval(TimeSpan value, string description)
        {
            Value = value;
            Description = description;
        }

        public TimeSpan Value { get; set; }

        public string Description { get; set; }
    }
}