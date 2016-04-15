namespace IPFilter.Native
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    [StructLayout(LayoutKind.Sequential)]
    public class StartupInfo
    {
        public int cb;
        public IntPtr lpReserved = IntPtr.Zero;
        public IntPtr lpDesktop = IntPtr.Zero;
        public IntPtr lpTitle = IntPtr.Zero;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2 = IntPtr.Zero;
        public SafeFileHandle hStdInput = new SafeFileHandle(IntPtr.Zero, false);
        public SafeFileHandle hStdOutput = new SafeFileHandle(IntPtr.Zero, false);
        public SafeFileHandle hStdError = new SafeFileHandle(IntPtr.Zero, false);

        public StartupInfo()
        {
            this.dwY = 0;
            this.cb = Marshal.SizeOf(this);
        }

        public void Dispose()
        {
            // close the handles created for child process
            if (this.hStdInput != null && !this.hStdInput.IsInvalid)
            {
                this.hStdInput.Close();
                this.hStdInput = null;
            }

            if (this.hStdOutput != null && !this.hStdOutput.IsInvalid)
            {
                this.hStdOutput.Close();
                this.hStdOutput = null;
            }

            if (this.hStdError == null || this.hStdError.IsInvalid) return;

            this.hStdError.Close();
            this.hStdError = null;
        }
    }
}