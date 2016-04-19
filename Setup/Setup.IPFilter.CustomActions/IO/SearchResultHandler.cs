namespace IPFilter.Setup.CustomActions.IO
{
    using System.Runtime;
    using System.Security;

    /// <summary>
    /// Handles transforming file search results to an object of type <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public abstract class SearchResultHandler<TSource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultHandler&lt;TSource&gt;"/> class.
        /// </summary>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected SearchResultHandler() {}

        /// <summary>
        /// Creates the result object.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        [SecurityCritical]
        internal abstract TSource CreateObject(SearchResult result);

        [SecurityCritical]
        internal abstract bool IsResultIncluded(SearchResult result);
    }
}