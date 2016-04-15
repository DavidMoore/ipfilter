namespace IPFilter.Native
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public class ProcessInformation
    {
        public IntPtr hProcess = IntPtr.Zero;
        public IntPtr hThread = IntPtr.Zero;
        public int dwProcessId;
        public int dwThreadId;
    }
}