using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace IPFilter.Cli
{
    /// <summary>
    /// Automatically creates a temporary file in the system's
    /// temporary directory, removing the file from the system when it
    /// goes out of scope.
    /// </summary>
    /// <example>
    /// string filename;
    /// using(TempFile file = new TempFile())
    /// {
    ///     filename = file.File.FullName;
    ///     Console.WriteLine("About to write to temporary file: {0}", filename);
    ///     file.WriteAllText("This is some test text");
    /// }
    /// if( !File.Exists(filename ) MessageWindow.Show("Temp file was deleted!");
    /// </example>
    public class TempFile : IDisposable
    {
        readonly FileInfo fileInfo;

        /// <summary>
        /// Creates a new temporary file, which ensures the temporary
        /// file is deleted once the object is disposed.
        /// </summary>
        public TempFile()
        {
            fileInfo = new FileInfo(System.IO.Path.GetTempFileName());
        }

        /// <summary>
        /// Creates a temporary file, using the passed format string to
        /// create the name. Parameter 0 is the generated temporary filename.
        /// e.g. to create a temp file with a .jpg extension, the passed
        /// name should be "{0}.jpg"
        /// </summary>
        /// <param name="name">The string format for the name, taking {0} as the generated temp file name</param>
        public TempFile(string name) : this()
        {
            // Strip the extension
            var nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name);
            
            // Get the new name
            var newName = string.Format(CultureInfo.CurrentCulture, name, nameWithoutExtension);

            var path = fileInfo.DirectoryName == null ? newName : System.IO.Path.Combine(fileInfo.DirectoryName, newName);

            fileInfo.MoveTo( System.IO.Path.GetFullPath(path) );
        }
        
        /// <summary>
        /// Handle to the temporary file.
        /// </summary>
        public FileInfo File { get { return fileInfo; } }

        public string Path => fileInfo?.FullName;

        ~TempFile()
        {
            Dispose(false);
        }

        ///<summary>
        /// Deletes the temporary file once out of scope or disposed.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees up managed resources.
        /// </summary>
        /// <param name="disposing"></param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void Dispose(bool disposing)
        {
            if( fileInfo == null ) return;
            
            // If the file doesn't exist, we don't have to do anything.
            fileInfo.Refresh();
            if(!fileInfo.Exists ) return;

            try
            {
                fileInfo.Delete();
            }
            catch(IOException ioe)
            {
                try
                {
                    // If we can't delete the temp file now (likely because it's locked for some reason),
                    // we can schedule to delete it on reboot when the handles on it should be gone. We need to
                    // be an administrator to do this.
                    if (Win32Api.MoveFileEx(fileInfo.FullName, null, Win32Api.MoveFileFlags.DelayUntilReboot)) return;
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
                catch (Exception innerException)
                {
                    Trace.TraceError("Couldn't schedule delete of locked file '{0}' at reboot: {1}", fileInfo, innerException);
                }

                Trace.TraceWarning( "Couldn't delete temporary file '{0}': {1}", fileInfo, ioe);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning( "Couldn't delete temporary file '{0}': {1}", fileInfo, ex);
            } 
        }

        /// <summary>Opens a text file, reads all lines of the file, and then closes the file.</summary>
        /// <returns>The contents of the file</returns>
        public string ReadAllText()
        {
            return System.IO.File.ReadAllText(fileInfo.FullName);
        }

        /// <summary>Creates a new file, writes the specified string array to the file using the specified encoding, and then closes the file. If the target file already exists, it is overwritten.</summary>
        /// <param name="contents"></param>
        public void WriteAllText(string contents)
        {
            System.IO.File.WriteAllText(fileInfo.FullName, contents);
        }

        public FileStream OpenShareableRead()
        {
            return fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public FileStream OpenWrite()
        {
            return fileInfo.Open(FileMode.Open, FileAccess.Write, FileShare.Read);
        }
    }
}