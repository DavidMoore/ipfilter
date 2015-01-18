namespace IPFilter.ViewModels
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Threading;

    class MessageBoxHelper
    {
        public static MessageBoxResult Show(Dispatcher parent, string title, MessageBoxButton buttons, MessageBoxImage image, MessageBoxResult defaultButton, string message, params object[] args)
        {
            var formattedMessage = args == null || args.Length == 0 ? message : string.Format(CultureInfo.CurrentCulture, message, args);

            var options = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0;
            
            Window window = null;

            var result = parent.Invoke( DispatcherPriority.Normal, new Func<MessageBoxResult>(delegate
            {
                if (window == null)
                {
                    window = Application.Current.MainWindow;

                    //return MessageBox.Show(formattedMessage, title, buttons, image, defaultButton, options);
                }
                return MessageBox.Show(window, formattedMessage, title, buttons, image, defaultButton, options);
            }));

            return (MessageBoxResult)result;
        }
    }
}
