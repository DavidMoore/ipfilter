namespace IPFilter.Native
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using Microsoft.Win32.SafeHandles;

    [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public sealed class SafeLocalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeLocalMemHandle() : base(true) {}

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeLocalMemHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
        {
            base.SetHandle(existingHandle);
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(string stringSecurityDescriptor,
            int stringSDRevision, out SafeLocalMemHandle pSecurityDescriptor, IntPtr securityDescriptorSize);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        protected override bool ReleaseHandle()
        {
            return (LocalFree(base.handle) == IntPtr.Zero);
        }
    }
}