namespace IPFilter.Setup.CustomActions
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Text;
    using IO;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Win32 API interop functions through P/Invoke.
    /// </summary>
    public static class Win32Api
    {
        const string advancedWindows32BaseApi = "advapi32.dll";
        const string kernel32ClientBaseApi = "kernel32.dll";

        /// <summary>
        /// Contains functions for manipulating the Windows registry.
        /// </summary>
        public static class Registry
        {
            /// <summary>
            /// Override a registry hive key to another registry key.
            /// </summary>
            /// <param name="key">Handle of the key to override.</param>
            /// <param name="newKey">Handle to override key.</param>
            internal static void RedirectRegistryKey(SafeRegistryHandle key, SafeRegistryHandle newKey)
            {
                if (0 != RegOverridePredefKey(key, newKey))
                {
                    throw new Exception();
                }
            }
            
            /// <summary>
            /// Interop to RegOverridePredefKey.
            /// </summary>
            /// <param name="key">Handle to key to override.</param>
            /// <param name="newKey">Handle to override key.</param>
            /// <returns>0 if success.</returns>
            [DllImport(advancedWindows32BaseApi, EntryPoint = "RegOverridePredefKey", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
            static extern int RegOverridePredefKey(SafeRegistryHandle key, SafeRegistryHandle newKey);
        }

        /// <summary>
        /// Contains functions for file system operations.
        /// </summary>
        public static class IO
        {
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

            [DllImport(kernel32ClientBaseApi, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
            [ResourceExposure(ResourceScope.Machine)]
            internal static extern int GetLongPathName(String path, StringBuilder longPathBuffer, int bufferLength);

            [DllImport(kernel32ClientBaseApi, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
            [ResourceExposure(ResourceScope.Machine)]
            internal unsafe static extern int GetLongPathName(char* path, char* longPathBuffer, int bufferLength);

            [DllImport(kernel32ClientBaseApi, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
            [ResourceExposure(ResourceScope.None)]
            internal static extern SafeFindHandle FindFirstFile(String fileName, [In, Out] FindData data);

            [DllImport(kernel32ClientBaseApi, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
            [ResourceExposure(ResourceScope.None)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern  bool FindNextFile(SafeFindHandle hndFindFile, [In, Out, MarshalAs(UnmanagedType.LPStruct)] FindData lpFindFileData);

            [DllImport(kernel32ClientBaseApi)]
            [ResourceExposure(ResourceScope.None)]
            [return: MarshalAs(UnmanagedType.Bool)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal static extern bool FindClose(IntPtr handle);
        }

        /// <summary>
        /// Loads a module into this process.
        /// </summary>
        /// <param name="file">The file to load.</param>
        /// <returns>A handle to the loaded module.</returns>
        public static SafeLibraryHandle LoadLibrary(string file)
        {
            var handle = LoadLibraryEx(file, IntPtr.Zero, LoadLibraryExFlags.LoadWithAlteredSearchPath);

            if (handle == null) throw new LoadLibraryException(file, Marshal.GetLastWin32Error());

            return handle;
        }

        /// <summary>
        /// Loads a dynamic link library into this process.
        /// </summary>
        /// <remarks>Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded.</remarks>
        /// <param name="fileName">The name of the module.</param>
        /// <param name="reserved">This parameter is reserved for future use. It must be null (set to <see cref="IntPtr.Zero"/>).</param>
        /// <param name="flags">The action to be taken when loading the module.</param>
        /// <returns>
        /// If the function succeeds, the return value is a handle to the loaded module.
        /// If the function fails, the return value is NULL. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport(kernel32ClientBaseApi, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeLibraryHandle LoadLibraryEx(string fileName, IntPtr reserved, LoadLibraryExFlags flags);

        /// <summary>
        /// Frees the loaded dynamic-link library (DLL) module and, if necessary, decrements its reference count.
        /// When the reference count reaches zero, the module is unloaded from the address space of the calling process and the handle is no longer valid.
        /// </summary>
        /// <param name="moduleHandle">A handle to the loaded library module. The <see cref="LoadLibraryEx"/> function returns this handle.</param>
        /// <returns></returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport(kernel32ClientBaseApi, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr moduleHandle);

        [DllImport(kernel32ClientBaseApi, SetLastError = false)]
        [ResourceExposure(ResourceScope.Process)]
        internal static extern int SetErrorMode(int newMode);

        // TODO: Make into an enum
        /// <summary>
        /// <p>The system does not display the critical-error-handler message box. Instead, the system sends the error to the calling process.</p>
        /// <p>Best practice is that all applications call the process-wide <see cref="SetErrorMode"/> function with a parameter of <see cref="FailCriticalErrors"/> at startup. This is to prevent error mode dialogs from hanging the application.</p>
        /// </summary>
        internal const int FailCriticalErrors = 1;

        [DllImport(kernel32ClientBaseApi, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal unsafe static extern int GetFullPathName(char* path, int numBufferChars, char* buffer, IntPtr mustBeZero);

        [DllImport(kernel32ClientBaseApi, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.Machine)]
        internal unsafe static extern int GetFullPathName(String path, int numBufferChars, StringBuilder buffer, IntPtr mustBeZero);

        [DllImport(kernel32ClientBaseApi, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int FormatMessage(int dwFlags, IntPtr lpSource,
                    int dwMessageId, int dwLanguageId, StringBuilder lpBuffer,
                    int nSize, IntPtr vaListArguments);

        internal static readonly IntPtr Null = IntPtr.Zero;
    }
}