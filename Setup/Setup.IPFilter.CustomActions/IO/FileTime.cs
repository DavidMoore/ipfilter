namespace IPFilter.Setup.CustomActions.IO
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Contains a 64-bit value representing the number of 100-nanosecond intervals since January 1, 1601 (UTC).
    /// </summary>
    [Serializable, BestFitMapping(false)]
    [StructLayout(LayoutKind.Sequential)]
    internal struct FileTime
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileTime"/> struct.
        /// </summary>
        /// <param name="fileTime">The file time.</param>
        public FileTime(long fileTime)
        {
            FileTimeLow = (uint)fileTime;
            FileTimeHigh = (uint)(fileTime >> 32);
        }

        /// <summary>
        /// Converts the <see cref="FileTimeLow"/> and <see cref="FileTimeHigh"/> values to the number
        /// of 100-nanosecond intervals since January 1, 1601 (UTC).
        /// </summary>
        /// <returns></returns>
        public long ToTicks()
        {
            return ((long)FileTimeHigh << 32) + FileTimeLow;
        }

        /// <summary>
        /// Converts this <see cref="FileTime"/> to a <see cref="DateTime"/>.
        /// </summary>
        /// <returns></returns>
        public DateTime ToDateTime()
        {
            return DateTime.FromFileTime(ToTicks());
        }

        /// <summary>
        /// The low-order part of the file time.
        /// </summary>
        public uint FileTimeLow;

        /// <summary>
        /// The high-order part of the file time.
        /// </summary>
        public uint FileTimeHigh;
    }
}