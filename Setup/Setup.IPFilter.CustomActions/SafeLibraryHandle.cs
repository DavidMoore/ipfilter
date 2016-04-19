namespace IPFilter.Setup.CustomActions
{
    using System.Security;
    using System.Security.Permissions;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// Safe wrapper for a handle to a module loaded with <see cref="Win32Api.LoadLibraryEx"/>.
    /// </summary>
    [SecurityCritical, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeLibraryHandle"/> class.
        /// </summary>
        internal SafeLibraryHandle() : base(true) { }

        /// <summary>
        /// Frees the handle to the loaded module.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the handle is released successfully; otherwise, in the event of a catastrophic failure, <c>false</c>.
        /// In this case, it generates a releaseHandleFailed MDA Managed Debugging Assistant.
        /// </returns>
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return Win32Api.FreeLibrary(handle);
        }
    }
}