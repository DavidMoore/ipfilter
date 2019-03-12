using System.Runtime.InteropServices;

namespace IPFilter.Formats
{
    /// <summary>
    /// IP Addresses are in Big Endian order, while Windows is Little Endian. This structure
    /// allows us to read and store in Little Endian.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Address
    {
        [FieldOffset(0)]
        public readonly uint Value;

        [FieldOffset(3)]
        public readonly byte OctetA;

        [FieldOffset(2)]
        public readonly byte OctetB;

        [FieldOffset(1)]
        public readonly byte OctetC;

        [FieldOffset(0)]
        public readonly byte OctetD;

        public Address(uint address) : this()
        {
            Value = address;
        }

        public static implicit operator uint(Address address)
        {
            return address.Value;
        }

        public static implicit operator Address(uint address)
        {
            return new Address(address);
        }

        /// <summary>
        /// Loopback range 127.0.0 to 127.255.255.255
        /// </summary>
        public bool IsLoopback => OctetA == 127;

        /// <summary>
        /// Address range 0.0.0.0 to 0.255.255.255
        /// </summary>
        public bool IsReserved => OctetA == 0;

        public bool IsPrivate => OctetA == 10 // 10.0.0.0 - 10.255.255.255
                                 || (OctetA == 172 && (OctetB >= 16 && OctetB <= 31)) // 172.16.0.0 - 172.31.255.255
                                 || (OctetA == 169 && OctetB == 254) // 169.254.0.0 - 169.254.255.255
                                 || (OctetA == 192 && OctetB == 168); // 192.168.0.0 - 192.168.255.255
    }
}