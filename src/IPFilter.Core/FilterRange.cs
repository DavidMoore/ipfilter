namespace IPFilter.Core;

public struct FilterRange
{
    public InternetAddress From { get; set; }
    public InternetAddress To { get; set; }
    public FilterRange(InternetAddress from, InternetAddress to)
    {
        From = from;
        To = to;
    }
    public static implicit operator FilterRange((InternetAddress from, InternetAddress to) range) => new FilterRange(range.from, range.to);
    public override string ToString() => $"[{From} - {To}]";
}