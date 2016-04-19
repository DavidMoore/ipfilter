namespace IPFilter.Setup.CustomActions.IO
{
    using System;

    /// <summary>
    /// File attributes are metadata values stored by the file system on disk and are used by the system and are
    /// available to developers via various file I/O APIs.
    /// </summary>
    [Flags]
    public enum FileAttributes
    {
        /// <summary>
        /// No file attributes.
        /// </summary>
        None = 0,

        /// <summary>
        /// A file that is read-only. Applications can read the file, but cannot write to it or delete it.
        /// This attribute is not honored on directories.
        /// For more information, see <see cref="http://go.microsoft.com/FWLink/?LinkId=125896">You cannot view or change the Read-only or the System attributes of folders in Windows Server 2003, in Windows XP, or in Windows Vista</see>.
        /// </summary>
        ReadOnly = 0x00000001,

        /// <summary>
        /// The file or directory is hidden. It is not included in an ordinary directory listing.
        /// </summary>
        Hidden = 0x00000002,

        /// <summary>
        /// A file or directory that the operating system uses a part of, or uses exclusively.
        /// </summary>
        System = 0x00000004,

        /// <summary>
        /// The handle that identifies a directory.
        /// </summary>
        Directory = 0x00000010,

        /// <summary>
        /// A file or directory that is an archive file or directory. Applications typically use this attribute to mark files for backup or removal.
        /// </summary>
        Archive = 0x00000020,

        /// <summary>
        /// This value is reserved for system use.
        /// </summary>
        Device = 0x00000040,

        /// <summary>
        /// A file that does not have other attributes set. This attribute is valid only when used alone.
        /// </summary>
        Normal = 0x00000080,

        /// <summary>
        /// A file that is being used for temporary storage. File systems avoid writing data back to mass storage if sufficient cache memory is available, because typically, an application deletes a temporary file after the handle is closed. In that scenario, the system can entirely avoid writing the data. Otherwise, the data is written after the handle is closed.
        /// </summary>
        Temporary = 0x00000100,

        /// <summary>
        /// A file that is a sparse file.
        /// </summary>
        SparseFile = 0x00000200,

        /// <summary>
        /// A file or directory that has an associated reparse point, or a file that is a symbolic link.
        /// </summary>
        ReparsePoint = 0x00000400,

        /// <summary>
        /// A file or directory that is compressed. For a file, all of the data in the file is compressed. For a directory, compression is the default for newly created files and subdirectories.
        /// </summary>
        Compressed = 0x00000800,

        /// <summary>
        /// The data of a file is not available immediately. This attribute indicates that the file data is physically moved to offline storage. This attribute is used by Remote Storage, which is the hierarchical storage management software. Applications should not arbitrarily change this attribute.
        /// </summary>
        Offline = 0x00001000,

        /// <summary>
        /// The file or directory is not to be indexed by the content indexing service.
        /// </summary>
        NotContentIndexed = 0x00002000,

        /// <summary>
        /// A file or directory that is encrypted. For a file, all data streams in the file are encrypted. For a directory, encryption is the default for newly created files and subdirectories.
        /// </summary>
        Encrypted = 0x00004000,

        /// <summary>
        /// This value is reserved for system use.
        /// </summary>
        Virtual = 0x00010000
    }
}