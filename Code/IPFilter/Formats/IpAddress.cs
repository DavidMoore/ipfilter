using System;
using System.Globalization;

namespace IPFilter.Formats
{
    public class IpAddress
    {
        public static uint Parse(string address)
        {
            if (string.IsNullOrEmpty(address)) return 0;

            if (address.IndexOf(':') > -1)
            {
                throw new NotSupportedException("IPv6 addresses not supported yet");
            }

            // Treat as an IPv4 address
            var numbers = address.Split(new[] { '.' }, 4);
            if (numbers.Length < 4) return 0;

            var bytes = new byte[4];

            for (var i = 0; i < numbers.Length; i++)
            {
                if (!int.TryParse(numbers[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)) return 0;
                if (number < 0 || number > 255) return 0;

                // Bytes will be reversed
                bytes[numbers.Length - 1 - i] = (byte)number;
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static byte[] GetBytes(uint address)
        {
            return !BitConverter.IsLittleEndian ? BitConverter.GetBytes(address) : BitConverter.GetBytes(ReverseBytes((int)address));
        }

        public static int ReverseBytes(int host)
        {
            return ((int)ReverseBytes((short)host) & (int)ushort.MaxValue) << 16 | (int)ReverseBytes((short)(host >> 16)) & (int)ushort.MaxValue;
        }

        public static uint ReverseBytes(uint host)
        {
            return (uint) (((int)ReverseBytes((short)host) & (int)ushort.MaxValue) << 16 | (int)ReverseBytes((short)(host >> 16)) & (int)ushort.MaxValue);
        }
        
        public static short ReverseBytes(short host)
        {
            return (short)(((int)host & (int)byte.MaxValue) << 8 | (int)host >> 8 & (int)byte.MaxValue);
        }
    }
}