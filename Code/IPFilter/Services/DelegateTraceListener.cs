namespace IPFilter.Services
{
    using System;
    using System.Diagnostics;

    public class DelegateTraceListener : TraceListener
    {
        readonly Action<string> writeAction;
        readonly Action<string> writeLineAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Diagnostics.TraceListener"/> class.
        /// </summary>
        public DelegateTraceListener(Action<string> writeAction, Action<string> writeLineAction)
        {
            this.writeAction = writeAction;
            this.writeLineAction = writeLineAction;
        }

        /// <summary>
        /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write. </param>
        public override void Write(string message)
        {
            writeAction?.Invoke(message);
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write. </param>
        public override void WriteLine(string message)
        {
            writeLineAction?.Invoke(message);
        }
    }
}