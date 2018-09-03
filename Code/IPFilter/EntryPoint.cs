using System.Net;

namespace IPFilter
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Apps;
    using Models;
    using Services;
    using Views;

    static class EntryPoint
    {
        static Assembly assembly = typeof (EntryPoint).Assembly;

        [STAThread]
        internal static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve +=  CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (args.Length > 0)
            {
                var commandLine = string.Join(" ", args);

                if (commandLine.IndexOf("/silent", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    try
                    {
                        Trace.TraceInformation("Running IPFilter in silent mode");
                        Console.WriteLine("Running IPFilter in silent mode");
                        SilentMain().GetAwaiter().GetResult();
                    }
                    catch (AggregateException ae)
                    {
                        Trace.TraceWarning("There were one or more errors trying to update the filter: ");

                        foreach (var exception in ae.InnerExceptions)
                        {
                            Trace.TraceWarning(exception.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning("There was a problem when trying to update the filter: " + ex);
                    }
                }
                else
                {
                    Trace.TraceWarning("Invalid command line: " + commandLine);
                }

                return;
            }

            var window = new MainWindow();
            var app = new App();
            app.Run(window);
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            if (Application.Current == null || Application.Current.MainWindow == null) return;

            var ex = args.ExceptionObject as Exception;
            if (ex == null) return;

            MessageBox.Show(Application.Current.MainWindow, ex.ToString(), "Unhandled Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var resourceName = "IPFilter.Assemblies." + new AssemblyName(args.Name).Name + ".dll";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return null;

                var assemblyData = new byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }

        static async Task SilentMain()
        {
            var detector = new ApplicationEnumerator();

            var apps = (await detector.GetInstalledApplications()).ToList();

            if (!apps.Any())
            {
                Trace.TraceWarning("No BitTorrent applications found. Nothing to do, so exiting.");
                return;
            }

            var cancellationSource = new CancellationTokenSource();

            // Download the filter
            var downloader = new FilterDownloader();
            var progress = new Progress<ProgressModel>(delegate(ProgressModel model)
            {
                Trace.TraceInformation("{0}", model.Caption);
            });

            using (var filter = await downloader.DownloadFilter(null, cancellationSource.Token, progress))
            {
                if (filter.Exception != null) throw filter.Exception;

                foreach (var application in apps)
                {
                    Trace.TraceInformation("Updating app {0} {1}", application.Description, application.Version);

                    await application.Application.UpdateFilterAsync(filter, cancellationSource.Token, progress);
                }
            }

            Trace.TraceInformation("Done.");
        }
    }
}