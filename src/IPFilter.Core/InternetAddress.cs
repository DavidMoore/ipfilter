namespace IPFilter.Core;

public struct InternetAddress
{
    public uint Address { get; set; }

    public InternetAddress(uint address)
    {
        Address = address;
    }

    public static implicit operator InternetAddress(uint address) => new InternetAddress(address);

    public static implicit operator uint(InternetAddress ipAddress) => ipAddress.Address;

    public override string ToString() => $"{(Address >> 24) & 0xFF}.{(Address >> 16) & 0xFF}.{(Address >> 8) & 0xFF}.{Address & 0xFF}";

    public static uint Parse(ReadOnlySpan<char> address)
    {
        Span<uint> numbers = stackalloc uint[4];
        byte part = 0;
        foreach(var ch in address)
        {
            if(ch == '.')
            {
                part++;
                continue;
            }

            if(ch is < '0' or > '9')
                break;

            numbers[part] = (uint)(numbers[part] * 10 + (ch - '0'));
        }

        return (numbers[0] << 24) | ((numbers[1] & 0xff) << 16) | ((numbers[2] & 0xff) << 8) | (numbers[3] & 0xff);
    }
}