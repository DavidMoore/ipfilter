using System.Collections.Generic;

namespace IPFilter.Core
{
    public class IPAddressComparer : IComparer<uint>, IEqualityComparer<uint>
    {
        public int Compare(uint x, uint y)
        {
            var xAddress = (long) x;
            var yAddress = (long) y;

            if (xAddress == yAddress) return 0;

            var distance = xAddress - yAddress;

            if (distance > int.MaxValue) return int.MaxValue;
            if (distance < int.MinValue) return int.MinValue;

            return (int)distance;
        }

        public bool Equals(uint x, uint y)
        {
            return x == y;
        }

        public int GetHashCode(uint obj)
        {
            return obj.GetHashCode();
        }
    }
}