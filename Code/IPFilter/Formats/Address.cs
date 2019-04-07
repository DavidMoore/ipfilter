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

        [FieldOffset(4)]
        internal string StringValue;

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

        /// <summary>Converts an Internet address to its standard notation.</summary>
        /// <returns>A string that contains the IP address in IPv4 dotted-quad notation.</returns>
        public override unsafe string ToString()
        {
            if (StringValue != null) return StringValue;

            int num1 = 15;
            char* chPtr = stackalloc char[15];
            int num2 = (int) (this.Value >> 24 & (long) byte.MaxValue);
            do
            {
                chPtr[--num1] = (char) (48 + num2 % 10);
                num2 /= 10;
            } while (num2 > 0);

            int num3;
            chPtr[num3 = num1 - 1] = '.';
            int num4 = (int) (this.Value >> 16 & (long) byte.MaxValue);
            do
            {
                chPtr[--num3] = (char) (48 + num4 % 10);
                num4 /= 10;
            } while (num4 > 0);

            int num5;
            chPtr[num5 = num3 - 1] = '.';
            int num6 = (int) (this.Value >> 8 & (long) byte.MaxValue);
            do
            {
                chPtr[--num5] = (char) (48 + num6 % 10);
                num6 /= 10;
            } while (num6 > 0);

            int startIndex;
            chPtr[startIndex = num5 - 1] = '.';
            int num7 = (int) (this.Value & (long) byte.MaxValue);
            do
            {
                chPtr[--startIndex] = (char) (48 + num7 % 10);
                num7 /= 10;
            } while (num7 > 0);

            StringValue = new string(chPtr, startIndex, 15 - startIndex);
            return StringValue;
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