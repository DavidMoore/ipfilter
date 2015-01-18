namespace IPFilter.ViewModels
{
    using System.Globalization;
    using System.Windows;

    class MessageBoxHelper
    {
        public static MessageBoxResult Show(string title, MessageBoxButton buttons, MessageBoxImage image, MessageBoxResult defaultButton, string message, params object[] args)
        {
            var formattedMessage = args == null || args.Length == 0 ? message : string.Format(CultureInfo.CurrentCulture, message, args);

            var options = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0;

            return MessageBox.Show(formattedMessage, title, buttons, image, defaultButton, options);
        }
    }
}
