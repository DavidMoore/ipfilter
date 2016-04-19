namespace IPFilter.Setup.CustomActions
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using IO;
    using FileAttributes = System.IO.FileAttributes;

    public class DeleteHelper
    {
        public static void DeleteFileSystemInfoWithSchedulingIfNecessary(FileSystemInfo info)
        {
            var isDirectory = (info.Attributes & FileAttributes.Directory) != 0;

            Exception exception;

            try
            {
                // We must recursively delete all the files and folders in the directory, because we
                // can't do a scheduled delete on a directory that isn't empty.
                if (isDirectory)
                {
                    foreach (var file in new FileSystemEnumerator<FileSystemInfo>(info.FullName, info.FullName, "*", SearchOption.TopDirectoryOnly, new FileSystemInfoResultHandler()))
                    {
                        DeleteFileSystemInfoWithSchedulingIfNecessary(file);
                    }

                    // Now we can try deleting the directory
                    new DirectoryInfo(info.FullName).Delete(true);
                }
                else
                {
                    // Delete the file if it exists, ensuring that any read-only
                    // flag is toggled off first to prevent exceptions.
                    var file = new FileInfo(info.FullName);
                    file.Refresh();
                    if (!file.Exists) return;
                    file.IsReadOnly = false;
                    file.Delete();
                    Trace.WriteLine("Successfully deleted file: " + file.FullName);
                }

                return;
            }
            catch (UnauthorizedAccessException uae)
            {
                exception = uae;
            }
            catch (IOException ioe)
            {

                switch (Marshal.GetHRForException(ioe))
                {
                    case -2147024894: // File doesn't exist
                        return;
                }
                exception = ioe;
            }

            if (Win32Api.IO.MoveFileEx(info.FullName, null, Win32Api.IO.MoveFileFlags.DelayUntilReboot))
            {
                Trace.WriteLine("Successfully scheduled deletion for file that is currently locked: " + info.FullName);
                return;
            }
            throw new InvalidOperationException(string.Format("Couldn't schedule delete of {0} '{1}' at reboot.",
                isDirectory ? "directory" : "file", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error())), exception);
        }
    }
}