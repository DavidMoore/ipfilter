namespace IPFilter.UI
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Windows.Threading;

    public class StringListTraceListener : TraceListener
    {
        readonly ObservableCollection<string> list;
        readonly Dispatcher dispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Diagnostics.TraceListener"/> class.
        /// </summary>
        public StringListTraceListener(ObservableCollection<string> list, Dispatcher dispatcher)
        {
            this.list = list;
            this.dispatcher = dispatcher;            
        }

        /// <summary>
        /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write. </param>
        public override void Write(string message)
        {
            //dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => list.Add(message)));
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write. </param>
        public override void WriteLine(string message)
        {
            dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => list.Add(message)));
        }
    }
}