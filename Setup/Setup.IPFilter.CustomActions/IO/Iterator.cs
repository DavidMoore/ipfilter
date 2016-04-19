namespace IPFilter.Setup.CustomActions.IO
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Security;
    using System.Threading;

    /// <summary>
    /// Abstract base iterator.
    /// </summary>
    /// <typeparam name="TSource">The type of the iteration source.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerator<TSource>
    {
        readonly int threadId;
        internal TSource current;
        internal int state;

        protected Iterator()
        {
            threadId = Thread.CurrentThread.ManagedThreadId;
        }
        
        [SecuritySafeCritical]
        public IEnumerator<TSource> GetEnumerator()
        {
            if( (threadId == Thread.CurrentThread.ManagedThreadId) && (state == 0) )
            {
                state = 1;
                return this;
            }
            var iterator = Clone();
            iterator.state = 1;
            return iterator;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [SecuritySafeCritical]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecuritySafeCritical]
        public abstract bool MoveNext();

        public virtual void Reset()
        {
            throw new NotSupportedException();
        }

        public TSource Current
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        protected abstract Iterator<TSource> Clone();

        protected virtual void Dispose(bool disposing)
        {
            current = default(TSource);
            state = -1;
        }
    }
}