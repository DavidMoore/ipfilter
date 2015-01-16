namespace IPFilter.UI
{
    using System;
    using System.Deployment.Application;
    using System.Diagnostics;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Windows;

    class DeploymentHelper
    {
        public static void Restart()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                var applicationFullName = ApplicationDeployment.CurrentDeployment.UpdatedApplicationFullName;

                // TODO: Auto-detect host type
                var hostType = HostType.CorFlag;

                Application.Current.Shutdown();
                CorLaunchApplication(hostType, applicationFullName, 0, null, 0, null, new ProcessInformation());
            }
            else
            {
                var commandLineArgs = Environment.GetCommandLineArgs();
                var stringBuilder = new StringBuilder((commandLineArgs.Length - 1)*16);
                for (var index = 1; index < commandLineArgs.Length - 1; ++index)
                {
                    stringBuilder.Append('"');
                    stringBuilder.Append(commandLineArgs[index]);
                    stringBuilder.Append("\" ");
                }
                if (commandLineArgs.Length > 1)
                {
                    stringBuilder.Append('"');
                    stringBuilder.Append(commandLineArgs[commandLineArgs.Length - 1]);
                    stringBuilder.Append('"');
                }
                var startInfo = Process.GetCurrentProcess().StartInfo;

                if (stringBuilder.Length > 0) startInfo.Arguments = stringBuilder.ToString();

                Application.Current.Shutdown();

                Process.Start(startInfo);
            }
        }

        [DllImport("clr.dll", CharSet = CharSet.Unicode, BestFitMapping = false, PreserveSig = false)]
        static extern void CorLaunchApplication(HostType hostType, string applicationFullName, int manifestPathsCount, string[] manifestPaths, int activationDataCount, string[] activationData, ProcessInformation processInformation);

        [SuppressUnmanagedCodeSecurity]
        [StructLayout(LayoutKind.Sequential)]
        internal class ProcessInformation
        {
            public IntPtr hProcess = IntPtr.Zero;
            public IntPtr hThread = IntPtr.Zero;
            static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            ~ProcessInformation()
            {
                Close();
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            void Close()
            {
                if (hProcess != (IntPtr) 0 && hProcess != INVALID_HANDLE_VALUE)
                {
                    CloseHandle(new HandleRef(this, hProcess));
                    hProcess = INVALID_HANDLE_VALUE;
                }
                if (!(hThread != (IntPtr) 0) || !(hThread != INVALID_HANDLE_VALUE)) return;
                CloseHandle(new HandleRef(this, hThread));
                hThread = INVALID_HANDLE_VALUE;
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            static extern bool CloseHandle(HandleRef handle);
        }

        internal enum HostType : uint
        {
            /// <summary>
            ///     Same as <see cref="AppLaunch" />.
            /// </summary>
            Default = 0,

            /// <summary>
            ///     <p>Launch the application from AppLaunch.exe.</p>
            ///     <p>Use this value for partially-trusted applications.</p>
            /// </summary>
            AppLaunch = 0x1,

            /// <summary>
            ///     <p>Launch the application directly. That is, launch the application from its own .exe file.</p>
            ///     <p>Use this value for fully-trusted applications.</p>
            /// </summary>
            CorFlag = 0x2
        }
    }
}