namespace IPFilter.Native
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public class SecurityAttributes
    {
        public int nLength = 12;
        public SafeLocalMemHandle lpSecurityDescriptor = new SafeLocalMemHandle(IntPtr.Zero, false);
        public bool bInheritHandle;
    }
}