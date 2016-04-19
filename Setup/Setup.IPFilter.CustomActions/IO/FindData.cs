namespace IPFilter.Setup.CustomActions.IO
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Contains information about the file that is found by the
    /// <see cref="Win32Api.IO.FindFirstFile"/>, FindFirstFileEx, or <see cref="Win32Api.IO.FindNextFile"/> functions.
    /// </summary>
    /// <remarks>
    /// <para>If a file has a long file name, the complete name appears in the <see cref="FileName"/> member,
    /// and the 8.3 format truncated version of the name appears in the <see cref="AlternateFileName"/> member.
    /// Otherwise, <see cref="AlternateFileName"/> is empty. If the FindFirstFileEx function was called with a
    /// value of FindExInfoBasic in the fInfoLevelId parameter, the <see cref="AlternateFileName"/> member will
    /// always contain a NULL string value. This remains true for all subsequent calls to the
    /// FindNextFile function. As an alternative method of retrieving the 8.3 format version of a file name,
    /// you can use the GetShortPathName function. For more information about file names, see File Names, 
    /// Paths, and Namespaces.</para>
    /// <para>Not all file systems can record creation and last access times, and not all file systems 
    /// record them in the same manner. For example, on the FAT file system, create time has a resolution
    ///  of 10 milliseconds, write time has a resolution of 2 seconds, and access time has a resolution of
    ///  1 day. The NTFS file system delays updates to the last access time for a file by up to 1 hour after
    ///  the last access. For more information, see File Times.</para>
    /// <para>See: http://msdn.microsoft.com/en-us/library/aa365740.aspx </para>
    /// </remarks>
    [Serializable, BestFitMapping(false)]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class FindData
    {
        const uint maxdword = 0xFFFFFFFF;

        /// <summary>
        /// The file attributes of the file.
        /// </summary>
        internal FileAttributes fileAttributes;

        /// <summary>
        /// A <see cref="FileTime"/> structure that specifies when a file or directory was created.
        /// <p>If the underlying file system does not support creation time, this member is zero.</p>
        /// </summary>
        internal FileTime CreationTime;

        /// <summary>
        /// <para>For a file, the structure specifies when the file was last read from, written to, or for executable files, run.</para>
        /// <para>For a directory, the structure specifies when the directory is created. If the underlying file system does not support last access time, this member is zero.</para>
        /// <para>On the FAT file system, the specified date for both files and directories is correct, but the time of day is always set to midnight.</para>
        /// </summary>
        internal FileTime LastAccessTime;

        /// <summary>
        /// <para>For a file, the structure specifies when the file was last written to, truncated, or overwritten, for example, when WriteFile or SetEndOfFile are used. The date and time are not updated when file attributes or security descriptors are changed.</para>
        /// <para>For a directory, the structure specifies when the directory is created. If the underlying file system does not support last write time, this member is zero.</para>
        /// </summary>
        internal FileTime LastWriteTime;

        /// <summary>
        /// The high-order DWORD value of the file size, in bytes.
        /// This value is zero unless the file size is greater than <see cref="maxdword"/>.
        /// The size of the file is equal to <code>(<see cref="FileSizeHigh"/> * (<see cref="maxdword"/> + 1)) + <see cref="FileSizeLow"/></code>.
        /// </summary>
        /// <see cref="FileSize"/>
        internal int FileSizeHigh;

        /// <summary>
        /// The low-order DWORD value of the file size, in bytes.
        /// </summary>
        /// <see cref="FileSizeHigh"/>
        internal int FileSizeLow;
        
        /// <summary>
        /// <p>If the <see cref="fileAttributes"/> member includes the <see cref="FileAttributes.ReparsePoint"/> attribute,
        /// this member specifies the reparse point tag.</p>
        /// 
        /// <p>Otherwise, this value is undefined and should not be used.</p>
        /// 
        /// <p>For more information see Reparse Point Tags.
        /// IO_REPARSE_TAG_DFS (0x8000000A)
        /// IO_REPARSE_TAG_DFSR (0x80000012)
        /// IO_REPARSE_TAG_HSM (0xC0000004)
        /// IO_REPARSE_TAG_HSM2 (0x80000006)
        /// IO_REPARSE_TAG_MOUNT_POINT (0xA0000003)
        /// IO_REPARSE_TAG_SIS (0x80000007)
        /// IO_REPARSE_TAG_SYMLINK (0xA000000C)
        /// </p>
        /// </summary>
        internal int dwReserved0;

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        internal int dwReserved1;

        /// <summary>
        /// The name of the file.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PathHelperMethods.MaxPath)]
        internal string FileName;

        /// <summary>
        /// An alternative name for the file.
        /// <p>This name is in the classic 8.3 file name format.</p>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        internal string AlternateFileName;

        /// <summary>
        /// Gets the size of the file.
        /// </summary>
        /// <value>The size of the file.</value>
        public long FileSize
        {
            get { return (FileSizeHigh * (maxdword + (long)1)) + FileSizeLow; }
        }

        /// <summary>
        /// Gets a value indicating whether this result is a file.
        /// </summary>
        /// <value><c>true</c> if this result is a file; otherwise, <c>false</c>.</value>
        public bool IsFile
        {
            get { return 0 == (fileAttributes & FileAttributes.Directory); }
        }

        /// <summary>
        /// Gets a value indicating whether this result is a directory.
        /// </summary>
        /// <value><c>true</c> if this result is a directory; otherwise, <c>false</c>.</value>
        public bool IsDir
        {
            get
            {
                // Don't add "." nor ".."
                return (0 != (fileAttributes & FileAttributes.Directory)) && !FileName.Equals(".") && !FileName.Equals("..");
            }
        }
    }
}