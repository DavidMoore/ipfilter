using System.Collections.Generic;
using System.IO;
using System.Net;
using IPFilter.Cli;
using IPFilter.Commands;
using IPFilter.Core;
using IPFilter.Logging;

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
        static readonly Assembly assembly = typeof (EntryPoint).Assembly;

        internal static readonly Version Version = new Version(Process.GetCurrentProcess().MainModule.FileVersionInfo.FileVersion);

        [STAThread]
        internal static void Main(string[] args)
        {
            if (Trace.Listeners["file"] == null)
            {
                Trace.AutoFlush = true;
                Trace.UseGlobalLock = false;
                Trace.IndentSize = 2;

                var listener = new FileTraceListener {Name = "file"};
                Trace.Listeners.Add(listener);
            }

            AppDomain.CurrentDomain.AssemblyResolve +=  CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (args.Length > 0)
            {
                var commandLine = string.Join(" ", args);
                Trace.TraceInformation("Arguments: " + commandLine);

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
                else if (commandLine.IndexOf("task", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    try
                    {
                        ScheduledTaskCommand.Execute();
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                        Console.WriteLine("Couldn't schedule task: \r\n" + ex);
                    }
                }
                else
                {


                    CurateList(args).GetAwaiter().GetResult();
                    //Trace.TraceWarning("Invalid command line: " + commandLine);
                }

                return;
            }

            var window = new MainWindow();
            var app = new App();
            app.Run(window);
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var ex = args.ExceptionObject as Exception;
            if (ex == null) return;

            Trace.TraceError(ex.ToString());

            if (Application.Current == null || Application.Current.MainWindow == null) return;
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

        static async Task CurateList(string[] args)
        {
            try
            {
                var options = Options.Parse(args);

                var context = new FilterContext();

                // Configure outputs
                if (options.Outputs.Count > 0)
                {
                    context.Filter = new TextFilterWriter(options.Outputs.First());
                }
                else
                {
                    // Output to the current directory by default
                    context.Filter = new TextFilterWriter( Path.GetFullPath(@".\ipfilter.dat"));
                }
            
                // Resolve the input URIs to nodes to visit
                var nodes = new List<UriNode>();

                foreach (var input in options.Inputs)
                {
                    var uri = context.UriResolver.Resolve(input);
                    if (uri == null) continue;
                    nodes.Add(new UriNode(uri));
                }

                using (INodeVisitor visitor = new NodeVisitor(context))
                {
                    Console.WriteLine("Acquiring list(s)...");
                    foreach (var node in nodes)
                    {
                        await visitor.Visit(node);
                    }

                    Console.WriteLine("Flushing...");
                    await visitor.Context.Filter.Flush();

                    Console.WriteLine("Written.");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }
    }
}