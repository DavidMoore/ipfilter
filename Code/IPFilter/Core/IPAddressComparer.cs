using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace IPFilter.Core
{
    public class IPAddressComparer : IComparer<IPAddress>, IEqualityComparer<IPAddress>
    {
        public int Compare(IPAddress x, IPAddress y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            if (x.AddressFamily != AddressFamily.InterNetwork || y.AddressFamily != AddressFamily.InterNetwork) throw new ArgumentException("Only IPv4 is supported");

#pragma warning disable 618 // We only support IPv4 so far so it's ok to use the integer address
            var xAddress = (long) (uint)IPAddress.NetworkToHostOrder( (int)x.Address);
            var yAddress = (long) (uint) IPAddress.NetworkToHostOrder( (int)y.Address);

            if (xAddress == yAddress) return 0;

            var distance = xAddress - yAddress;
            if (distance > int.MaxValue) return int.MaxValue;
            if (distance < int.MinValue) return int.MinValue;
            return (int)distance;
#pragma warning restore 618
        }

        public bool Equals(IPAddress x, IPAddress y)
        {
            if (x == null && y == null) return false;
            if (x == null || y == null) return false;
            if (ReferenceEquals(x, y)) return true;
            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(IPAddress obj)
        {
            return IPAddress.NetworkToHostOrder((int) obj.Address);
        }
    }
}