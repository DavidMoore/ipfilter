using System;
using System.Runtime.InteropServices;

namespace IPFilter.Cli
{
    static class Win32Api
    {
        const string kernel32ClientBaseApi = "kernel32.dll";

        /// <summary>
        /// Moves an existing file or directory, including its children, with various move options.
        /// </summary>
        /// <param name="existingFileName">
        /// <p>The current name of the file or directory on the local computer.</p>
        /// <p>If <paramref name="moveFileFlags"/> specifies <see cref="MoveFileFlags.DelayUntilReboot"/>,
        /// the file cannot exist on a remote share, because delayed operations are performed
        /// before the network is available.</p>
        /// </param>
        /// <param name="newFileName">
        /// <p>The new name of the file or directory on the local computer.</p>
        /// <p>When moving a file, the destination can be on a different file system or volume.
        /// If the destination is on another drive, you must set the <see cref="MoveFileFlags.CopyAllowed"/> flag in <paramref name="moveFileFlags"/>.</p>
        /// <p>When moving a directory, the destination must be on the same drive.</p>
        /// <p>If <paramref name="moveFileFlags"/> specifies <see cref="MoveFileFlags.DelayUntilReboot"/> and <paramref name="newFileName"/> is <c>null</c>, <see cref="MoveFileEx"/> registers the <paramref name="existingFileName"/> file to be deleted when the system restarts.
        /// If <paramref name="existingFileName"/> refers to a directory, the system removes the directory at restart only if the directory is empty.</p>
        /// </param>
        /// <param name="moveFileFlags">The options flags for the file or directory move. See <see cref="MoveFileFlags"/>.</param>
        /// <returns>
        /// <p>If the function succeeds, the return value is nonzero.</p>
        /// <p>If the function fails, the return value is zero (0). To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</p>
        /// </returns>
        [DllImport(kernel32ClientBaseApi, SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool MoveFileEx(string existingFileName, string newFileName, MoveFileFlags moveFileFlags);

        /// <summary>
        /// Flags for specifying options to <see cref="MoveFileEx"/>.
        /// </summary>
        [Flags]
        internal enum MoveFileFlags
        {
            /// <summary>
            /// <p>If a file named lpNewFileName exists, the function replaces its contents with the contents of the
            /// lpExistingFileName file, provided that security requirements regarding access control lists (ACLs) are met.
            /// For more information, see the Remarks section of this topic.</p>
            /// <p>This value cannot be used if newFileName or existingFileName names a directory.</p>
            /// </summary>
            ReplaceExisting = 0x00000001,

            /// <summary>
            /// If the file is to be moved to a different volume, the function simulates the move by using the
            /// CopyFile and DeleteFile functions.
            /// <p>This value cannot be used with <see cref="DelayUntilReboot"/>.</p>
            /// </summary>
            CopyAllowed = 0x00000002,

            /// <summary>
            /// <p>The system does not move the file until the operating system is restarted.
            /// The system moves the file immediately after AUTOCHK is executed, but before creating any paging files.
            /// Consequently, this parameter enables the function to delete paging files from previous startups.</p>
            /// <p>This value can be used only if the process is in the context of a user who belongs to the
            /// administrators group or the LocalSystem account.</p>
            /// <p>This value cannot be used with <see cref="CopyAllowed"/>.</p>
            /// </summary>
            DelayUntilReboot = 0x00000004,

            /// <summary>
            /// <p>The function does not return until the file is actually moved on the disk.</p>
            /// <p>Setting this value guarantees that a move performed as a copy and delete operation
            /// is flushed to disk before the function returns. The flush occurs at the end of the copy operation.</p>
            /// <p>This value has no effect if <see cref="DelayUntilReboot"/> is set.</p>
            /// </summary>
            WriteThrough = 0x00000008,

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            CreateHardlink = 0x00000010,

            /// <summary>
            /// The function fails if the source file is a link source, but the file cannot be tracked after the move.
            /// This situation can occur if the destination is a volume formatted with the FAT file system.
            /// </summary>
            FailIfNotTrackable = 0x00000020
        }
    }
}