namespace IPFilter.Setup.CustomActions.IO
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Security;

    public class StringResultHandler : SearchResultHandler<string>
    {
        readonly bool includeDirectories;
        readonly bool includeFiles;

        public StringResultHandler(bool includeFiles, bool includeDirectories)
        {
            this.includeFiles = includeFiles;
            this.includeDirectories = includeDirectories;
        }

        [SecurityCritical]
        internal override bool IsResultIncluded(SearchResult result)
        {
            var includeFile = includeFiles && result.FindData.IsFile;
            var includeDir = includeDirectories && result.FindData.IsDir;
            Contract.Assert(!(includeFile && includeDir), result.FindData.FileName + ": current item can't be both file and dir!");
            return (includeFile || includeDir);
        }

        [SecurityCritical]
        internal override String CreateObject(SearchResult result)
        {
            return result.UserPath;
        }
    }
}