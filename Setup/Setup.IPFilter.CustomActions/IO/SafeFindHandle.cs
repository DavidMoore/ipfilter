namespace IPFilter.Setup.CustomActions.IO
{
    using System.Security;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Safe handle for <see cref="Win32Api.IO.FindFirstFile"/> and <see cref="Win32Api.IO.FindNextFile"/> calls.
    /// </summary>
    [SecurityCritical]
    internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeFindHandle"/> class.
        /// </summary>
        [SecurityCritical]
        internal SafeFindHandle() : base(true) {}

        /// <summary>
        /// Frees the Win32 handle by calling <see cref="Win32Api.IO.FindClose"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the handle is released successfully; otherwise, in the event of a catastrophic failure, <c>false</c>.
        /// In this case, it generates a releaseHandleFailed MDA Managed Debugging Assistant.
        /// </returns>
        [SecurityCritical]
        override protected bool ReleaseHandle() 
        {
            return Win32Api.IO.FindClose(handle); 
        } 
    }
}