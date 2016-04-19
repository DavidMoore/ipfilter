namespace IPFilter.Setup.CustomActions.IO
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Security;

    /// <summary>
    /// Contains a search result from a find file call.
    /// </summary>
    sealed class SearchResult
    {
        [SecurityCritical] readonly FindData findData;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResult"/> class.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <param name="userPath">The user path.</param>
        /// <param name="findData">The find data.</param>
        [SecurityCritical]
        internal SearchResult(String fullPath, String userPath, FindData findData)
        {
            Contract.Requires(fullPath != null);
            Contract.Requires(userPath != null);

            FullPath = fullPath;
            UserPath = userPath;
            this.findData = findData;
        }

        internal string FullPath { get; private set; }

        internal string UserPath { get; private set; }

        internal FindData FindData
        {
            [SecurityCritical]
            get { return findData; }
        }
    }
}