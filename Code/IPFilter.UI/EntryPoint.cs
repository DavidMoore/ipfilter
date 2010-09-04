using System;

namespace IPFilter.UI
{
    internal static class EntryPoint
    {
        [STAThread]
        internal static void Main(string[] args)
        {
            var app = new App();

            var window = new MainWindow();

            app.Run(window);
        }
    }
}
