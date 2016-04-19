namespace IPFilter.Setup.CustomActions.IO
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    /// <summary>
    /// Helper methods for throwing appropriate exceptions for a
    /// Win32 error code related to a file system path.
    /// </summary>
    [Pure]
    static class ErrorHelper
    {
        [SecuritySafeCritical]
        internal static void WinIoError()
        {
            WinIoError(Marshal.GetLastWin32Error(), string.Empty);
        }
        
        /// <summary>
        /// Gets the displayable path. If we don't have discovery permissions
        /// for the path, we will return just the file name.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="isInvalidPath"><c>true</c> if <paramref name="path"/> is invalid.</param>
        /// <returns></returns>
        /// <exception cref="DictationException">if the path is a directory, and we don't have <see cref="FileIOPermissionAccess.PathDiscovery"/> permission to it.</exception>
        [SecurityCritical]
        static string GetDisplayablePath(string path, bool isInvalidPath)
        {
            if( string.IsNullOrWhiteSpace(path) ) return path;

            // Is it a fully qualified path?
            var isFullyQualified = false;
            if( path.Length < 2 ) return path;

            if( (PathHelperMethods.IsDirectorySeparator(path[0]) && PathHelperMethods.IsDirectorySeparator(path[1])) || path[1] == Path.VolumeSeparatorChar)
            {
                isFullyQualified = true;
            }

            if( !isFullyQualified && !isInvalidPath ) return path;
            
            try
            {
                if( !isInvalidPath )
                {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new[] {path}).Demand();
                    return path;
                }
            }
            catch( SecurityException ) {}
            catch( ArgumentException )
            {
                // ? and * characters cause ArgumentException to be thrown from HasIllegalCharacters
                // inside FileIOPermission.AddPathList 
            }
            catch( NotSupportedException )
            {
                // paths like "!Bogus\\dir:with/junk_.in it" can cause NotSupportedException to be thrown
                // from Security.Util.StringExpressionSet.CanonicalizePath when ':' is found in the path 
                // beyond string index position 1.
            }

            if( PathHelperMethods.IsDirectorySeparator(path[path.Length - 1]) )
            {
                throw new UnauthorizedAccessException($"No permission to directory name '{path}'");
            }
                    
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Takes a Win32 error code from <see cref="Marshal.GetLastWin32Error"/>, and
        /// a path, and throws an appropriate exception and message for the corresponding error.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorPath">The path related to the error.</param>
        [SecurityCritical]
        internal static void WinIoError(int errorCode, String errorPath)
        {
            var isInvalidPath = errorCode == Win32Error.ERROR_INVALID_NAME || errorCode == Win32Error.ERROR_BAD_PATHNAME;
            var path = GetDisplayablePath(errorPath, isInvalidPath);

            switch( errorCode )
            {
                case Win32Error.ERROR_FILE_NOT_FOUND:
                    throw new FileNotFoundException("File not found", path);

                case Win32Error.ERROR_PATH_NOT_FOUND:
                    throw new DirectoryNotFoundException("Path not found: " + path);

                case Win32Error.ERROR_ACCESS_DENIED:
                    throw new UnauthorizedAccessException("Access denied to path: " + path);

                case Win32Error.ERROR_ALREADY_EXISTS:
                    throw new IOException("Path already exists: " + path, Win32Error.MakeHRFromErrorCode(errorCode));

                case Win32Error.ERROR_FILENAME_EXCED_RANGE:
                    throw new PathTooLongException("Path too long: " + path);

                case Win32Error.ERROR_INVALID_DRIVE:
                    throw new DriveNotFoundException("Drive not found for path: " + path);

                case Win32Error.ERROR_INVALID_PARAMETER:
                    throw new IOException(Win32Error.GetMessage(errorCode), Win32Error.MakeHRFromErrorCode(errorCode));

                case Win32Error.ERROR_SHARING_VIOLATION:
                    throw new IOException("Sharing violation error for path: " + path, Win32Error.MakeHRFromErrorCode(errorCode));

                case Win32Error.ERROR_FILE_EXISTS:
                    throw new IOException("File exists error for path: " + path, Win32Error.MakeHRFromErrorCode(errorCode));

                case Win32Error.ERROR_OPERATION_ABORTED:
                    throw new OperationCanceledException();

                default:
                    throw new IOException(Win32Error.GetMessage(errorCode), Win32Error.MakeHRFromErrorCode(errorCode));
            }
        }
    }
}