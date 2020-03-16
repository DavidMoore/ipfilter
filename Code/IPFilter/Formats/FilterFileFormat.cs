using System;

namespace IPFilter.Formats
{
    [Flags]
    enum FilterFileFormat
    {
        None = 0,
        Emule = 1,
        P2p = 2,
        P2b = 4
    }
}