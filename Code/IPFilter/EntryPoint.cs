namespace IPFilter
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Apps;
    using Microsoft;
    using Models;
    using Properties;
    using Services;
    using Views;

    static class EntryPoint
    {
        [STAThread]
        internal static void Main(string[] args)
        {

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