using System;
using System.Runtime.InteropServices;

namespace IPFilter.Native
{
    public static class Win32Api
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool AttachThreadInput(uint threadToBeAttached, uint threadToBeAttachedTo, bool attach);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        static extern IntPtr SetFocus(IntPtr hWnd);

        public static void BringToFront(IntPtr handle)
        {
            // Get the handle of the current foreground window
            var foregroundWindow = GetForegroundWindow();

            if (foregroundWindow == IntPtr.Zero)
            {
                // If the foreground window can't be found (which can happen in some circumstances,
                // such as when focus is being switched, or screensaver is active), then the best
                // we can do is try to bring the requested window to the front anyway.
                SetForegroundWindow(handle);
                SetFocus(handle);
                return;
            }

            // If we're already the foreground window, set focus to make sure.
            if (foregroundWindow == handle)
            {
                SetFocus(handle);
                return;
            }

            // Get the thread handle for the window that's currently in the foreground.
            var windowThread = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);

            // Get our current thread handle.
            var currentThread = GetCurrentThreadId();

            try
            {
                if (currentThread != windowThread)
                {
                    // Attach our thread to the window that holds the thread input, so that
                    // we now have authority over the foreground window and focus.
                    AttachThreadInput(currentThread, windowThread, true);
                }

                // Move the requested window into the foreground, with focus.
                SetForegroundWindow(handle);
                SetFocus(handle);
            }
            finally
            {
                if (currentThread != windowThread)
                {
                    // Don't forget to detach our thread from the thread input afterwards.
                    AttachThreadInput(currentThread, windowThread, false);
                }
            }
        }
    }
}