namespace IPFilter
{
    using System;
    using System.Threading;

    /// <summary>
    ///     Provides an <see cref="T:System.IProgress`1" /> that invokes callbacks for each reported progress value.
    /// </summary>
    /// <typeparam name="T">Specifies the type of the progress report value.</typeparam>
    public class Progress<T> : IProgress<T>
    {
        readonly Action<T> handler;
        readonly SendOrPostCallback invokeHandlers;
        readonly SynchronizationContext synchronizationContext;

        /// <summary>
        ///     Initializes the <see cref="T:System.Progress`1" /> object.
        /// </summary>
        public Progress()
        {
            synchronizationContext = SynchronizationContext.Current;
            invokeHandlers = InvokeHandlers;
        }

        /// <summary>
        ///     Initializes the <see cref="T:System.Progress`1" /> object with the specified callback.
        /// </summary>
        /// <param name="handler">
        ///     A handler to invoke for each reported progress value. This handler will be invoked in addition to
        ///     any delegates registered with the <see cref="E:System.Progress`1.ProgressChanged" /> event. Depending on the
        ///     <see cref="T:System.Threading.SynchronizationContext" /> instance captured by the
        ///     <see cref="T:System.Progress`1" /> at construction, it is possible that this handler instance could be invoked
        ///     concurrently with itself.
        /// </param>
        public Progress(Action<T> handler) : this()
        {
            if (handler == null) throw new ArgumentNullException("handler");
            this.handler = handler;
        }

        void IProgress<T>.Report(T value)
        {
            OnReport(value);
        }

        /// <summary>
        ///     Raised for each reported progress value.
        /// </summary>
        public event EventHandler<T> ProgressChanged;

        /// <summary>
        ///     Reports a progress change.
        /// </summary>
        /// <param name="value">The value of the updated progress.</param>
        protected virtual void OnReport(T value)
        {
            if (handler == null && ProgressChanged == null) return;
            synchronizationContext.Post(invokeHandlers, value);
        }

        void InvokeHandlers(object state)
        {
            var e = (T) state;
            Action<T> action = handler;
            EventHandler<T> eventHandler = ProgressChanged;
            if (action != null) action(e);
            if (eventHandler == null) return;
            eventHandler(this, e);
        }
    }

    [Serializable]
    public delegate void EventHandler<TEventArgs>(object sender, TEventArgs e);
}