namespace IPFilter.Setup.CustomActions.IO
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;

    /// <summary>
    /// Specifies the parameters for a file search.
    /// </summary>
    sealed class SearchData
    {
        /// <summary>
        /// Fully-qualified version of the <see cref="userPath"/>, excluding the search criteria in the end e.g. C:\Foo\Bar.
        /// </summary>
        public readonly string fullPath;

        /// <summary>
        /// The search options for <see cref="userPath"/>.
        /// </summary>
        public readonly SearchOption searchOptions;

        /// <summary>
        /// User-specified search path. This may include wildcard characters, and not be fully qualified e.g. Foo\Bar or Foo\Bar\*.*
        /// </summary>
        public readonly string userPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchData"/> class.
        /// </summary>
        public SearchData() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchData"/> class.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        /// <param name="userPath">The user path.</param>
        /// <param name="searchOptions">The search options.</param>
        public SearchData(String fullPath, String userPath, SearchOption searchOptions)
        {
            Contract.Requires(fullPath != null);
            Contract.Requires(searchOptions == SearchOption.AllDirectories || searchOptions == SearchOption.TopDirectoryOnly);

            if( PathHelperMethods.IsDirectorySeparator(fullPath[fullPath.Length - 1]) )
            {
                this.fullPath = fullPath;
            }
            else
            {
                this.fullPath = fullPath + Path.DirectorySeparatorChar;
            }

            if( string.IsNullOrEmpty(userPath) || PathHelperMethods.IsDirectorySeparator(userPath[userPath.Length - 1]) )
            {
                this.userPath = userPath;
            }
            else
            {
                this.userPath = userPath + Path.DirectorySeparatorChar;
            }

            this.searchOptions = searchOptions;
        }
    }
}