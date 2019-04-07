using System;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;

namespace IPFilter.Formats
{
    public class IpAddress
    {
        public unsafe static uint Parse(string address)
        {
            fixed (char* characters = address)
            {
                int end = address.Length;
                var result = ParseNonCanonical(characters, 0, ref end, true);
                //var test = IPAddress.Parse(address).Address;
                if (end == address.Length) return (uint)result;
                return 0;
            }

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

        internal const long Invalid = -1;
        private const long MaxIPv4Value = uint.MaxValue; // the native parser cannot handle MaxIPv4Value, only MaxIPv4Value - 1
        private const int Octal = 8;
        private const int Decimal = 10;
        private const int Hex = 16;

        private const int NumberOfLabels = 4;

        // Parse any canonical or noncanonical IPv4 formats and return a long between 0 and MaxIPv4Value.
        // Return Invalid (-1) for failures.
        // If the address has less than three dots, only the rightmost section is assumed to contain the combined value for
        // the missing sections: 0xFF00FFFF == 0xFF.0x00.0xFF.0xFF == 0xFF.0xFFFF
        internal static unsafe long ParseNonCanonical(char* name, int start, ref int end, bool notImplicitFile)
        {
            char ch;
            long* parts = stackalloc long[4];
            long currentValue = 0;
            bool atLeastOneChar = false;

            // Parse one dotted section at a time
            int dotCount = 0; // Limit 3
            int current = start;
            for (; current < end; current++)
            {
                ch = name[current];
                currentValue = 0;

                // Figure out what base this section is in
                var numberBase = Decimal;
//                if (ch == '0')
//                {
//                    numberBase = Octal;
//                    current++;
//                    atLeastOneChar = true;
//                    if (current < end)
//                    {
//                        ch = name[current];
//                        if (ch == 'x' || ch == 'X')
//                        {
//                            numberBase = Hex;
//                            current++;
//                            atLeastOneChar = false;
//                        }
//                    }
//                }

                // Parse this section
                for (; current < end; current++)
                {
                    ch = name[current];
                    int digitValue;

                    if ((numberBase == Decimal || numberBase == Hex) && '0' <= ch && ch <= '9')
                    {
                        digitValue = ch - '0';
                    }
                    else if (numberBase == Octal && '0' <= ch && ch <= '7')
                    {
                        digitValue = ch - '0';
                    }
                    else if (numberBase == Hex && 'a' <= ch && ch <= 'f')
                    {
                        digitValue = ch + 10 - 'a';
                    }
                    else if (numberBase == Hex && 'A' <= ch && ch <= 'F')
                    {
                        digitValue = ch + 10 - 'A';
                    }
                    else
                    {
                        break; // Invalid/terminator
                    }

                    currentValue = (currentValue * numberBase) + digitValue;

                    if (currentValue > MaxIPv4Value) // Overflow
                    {
                        return Invalid;
                    }

                    atLeastOneChar = true;
                }

                if (current < end && name[current] == '.')
                {
                    if (dotCount >= 3 // Max of 3 dots and 4 segments
                        || !atLeastOneChar // No empty segmets: 1...1
                                           // Only the last segment can be more than 255 (if there are less than 3 dots)
                        || currentValue > 0xFF)
                    {
                        return Invalid;
                    }
                    parts[dotCount] = currentValue;
                    dotCount++;
                    atLeastOneChar = false;
                    continue;
                }
                // We don't get here unless We find an invalid character or a terminator
                break;
            }

            // Terminators
            if (!atLeastOneChar)
            {
                return Invalid;  // Empty trailing segment: 1.1.1.
            }

            if (current >= end)
            {
                // end of string, allowed
            }
            else if ((ch = name[current]) == '/' || ch == '\\' || (notImplicitFile && (ch == ':' || ch == '?' || ch == '#')))
            {
                end = current;
            }
            else
            {
                // not a valid terminating character
                return Invalid;
            }

            parts[dotCount] = currentValue;

            // Parsed, reassemble and check for overflows
            switch (dotCount)
            {
                case 0: // 0xFFFFFFFF
                    if (parts[0] > MaxIPv4Value) return Invalid;
                    return parts[0];
                case 1: // 0xFF.0xFFFFFF
                    if (parts[1] > 0xffffff) return Invalid;
                    return (parts[0] << 24) | (parts[1] & 0xffffff);
                case 2: // 0xFF.0xFF.0xFFFF
                    if (parts[2] > 0xffff) return Invalid;
                    return (parts[0] << 24) | ((parts[1] & 0xff) << 16) | (parts[2] & 0xffff);
                case 3: // 0xFF.0xFF.0xFF.0xFF
                    if (parts[3] > 0xff) return Invalid;
                    return (parts[0] << 24) | ((parts[1] & 0xff) << 16) | ((parts[2] & 0xff) << 8) | (parts[3] & 0xff);
                default:
                    return Invalid;
            }
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