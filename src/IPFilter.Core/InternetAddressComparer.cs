namespace IPFilter.Core;

public class InternetAddressComparer : IComparer<uint>, IEqualityComparer<uint>
{
    public int Compare(uint x, uint y)
    {
        if(x == y) return 0;
        var distance = (long)x - y;
        return distance switch
        {
            > int.MaxValue => int.MaxValue,
            < int.MinValue => int.MinValue,
            _ => (int)distance
        };
    }

    public bool Equals(uint x, uint y) => x == y;

    public int GetHashCode(uint obj) => obj.GetHashCode();
}