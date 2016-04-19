namespace IPFilter.Setup.CustomActions.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    /// <summary>
    /// Enumerates the file system by searching a specified path
    /// that can contain wild cards, and additionally
    /// search within sub directories.
    /// </summary>
    /// <typeparam name="TSource">The type of result to return.</typeparam>
    public class FileSystemEnumerator<TSource> : Iterator<TSource>
    {
        const int stateInit = 1;
        const int stateSearchNextDir = 2;
        const int stateFindNextFile = 3;
        const int stateFinish = 4;

        readonly SearchResultHandler<TSource> resultHandler;
        readonly String fullPath;
        readonly String normalizedSearchPath;
        readonly int oldMode;
        readonly String searchCriteria;
        readonly SearchOption searchOption;
        readonly List<SearchData> searchStack;
        readonly String userPath;
        [SecurityCritical] SafeFindHandle safeFindHandle;
        bool isEmpty;
        bool needsParentPathDiscoveryDemand;
        SearchData searchData;

        // Input to this method should already be fullpath. This method will ensure that we append 
        // the trailing slash only when appropriate and when thisDirOnly is specified append a "."
        // at the end of the path to indicate that the demand is only for the fullpath and not
        // everything underneath it.

        [SecuritySafeCritical]
        public FileSystemEnumerator(String path, String originalUserPath, String searchPattern, SearchOption searchOption, SearchResultHandler<TSource> resultHandler)
        {
            Contract.Requires(path != null);
            Contract.Requires(originalUserPath != null);
            Contract.Requires(searchPattern != null);
            Contract.Requires(searchOption == SearchOption.AllDirectories || searchOption == SearchOption.TopDirectoryOnly);
            Contract.Requires(resultHandler != null);

            oldMode = Win32Api.SetErrorMode(Win32Api.FailCriticalErrors);

            searchStack = new List<SearchData>();

            String normalizedSearchPattern = NormalizeSearchPattern(searchPattern);

            if( normalizedSearchPattern.Length == 0 )
            {
                isEmpty = true;
            }
            else
            {
                this.resultHandler = resultHandler;
                this.searchOption = searchOption;

                fullPath = PathHelperMethods.GetFullPathInternal(path);
                String fullSearchString = GetFullSearchString(fullPath, normalizedSearchPattern);
                normalizedSearchPath = Path.GetDirectoryName(fullSearchString);

                // permission demands
                var demandPaths = new String[2];
                // Any illegal chars such as *, ? will be caught by FileIOPermission.HasIllegalCharacters
                demandPaths[0] = GetDemandDir(fullPath, true);
                // For filters like foo\*.cs we need to verify if the directory foo is not denied access.
                // Do a demand on the combined path so that we can fail early in case of deny 
                demandPaths[1] = GetDemandDir(normalizedSearchPath, true);
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, demandPaths).Demand();

                // normalize search criteria
                searchCriteria = GetNormalizedSearchCriteria(fullSearchString, normalizedSearchPath);

                // fix up user path 
                String searchPatternDirName = Path.GetDirectoryName(normalizedSearchPattern);
                String userPathTemp = originalUserPath;
                if( searchPatternDirName != null && searchPatternDirName.Length != 0 )
                {
                    userPathTemp = Path.Combine(userPathTemp, searchPatternDirName);
                }
                userPath = userPathTemp;

                searchData = new SearchData(normalizedSearchPath, userPath, searchOption);

                CommonInit();
            }
        }

        [SecuritySafeCritical]
        FileSystemEnumerator(String fullPath, String normalizedSearchPath, String searchCriteria, String userPath, SearchOption searchOption, SearchResultHandler<TSource> resultHandler)
        {
            this.fullPath = fullPath;
            this.normalizedSearchPath = normalizedSearchPath;
            this.searchCriteria = searchCriteria;
            this.resultHandler = resultHandler;
            this.userPath = userPath;
            this.searchOption = searchOption;

            searchStack = new List<SearchData>();

            if( searchCriteria != null )
            {
                // permission demands 
                var demandPaths = new String[2];
                // Any illegal chars such as *, ? will be caught by FileIOPermission.HasIllegalCharacters
                demandPaths[0] = GetDemandDir(fullPath, true);
                // For filters like foo\*.cs we need to verify if the directory foo is not denied access.
                // Do a demand on the combined path so that we can fail early in case of deny
                demandPaths[1] = GetDemandDir(normalizedSearchPath, true);
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, demandPaths).Demand();

                searchData = new SearchData(normalizedSearchPath, userPath, searchOption);
                CommonInit();
            }
            else
            {
                isEmpty = true;
            }
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.None, ResourceScope.None)]
        internal static String GetDemandDir(string fullPath, bool thisDirOnly)
        {
            String demandPath;

            if( thisDirOnly )
            {
                if( fullPath.EndsWith(PathHelperMethods.DirectorySeparatorChar)
                    || fullPath.EndsWith(PathHelperMethods.AltDirectorySeparatorChar) )
                    demandPath = fullPath + '.';
                else
                    demandPath = fullPath + Path.DirectorySeparatorChar + '.';
            }
            else
            {
                if( !(fullPath.EndsWith(Path.DirectorySeparatorChar)
                      || fullPath.EndsWith(Path.AltDirectorySeparatorChar)) )
                    demandPath = fullPath + Path.DirectorySeparatorChar;
                else
                    demandPath = fullPath;
            }
            return demandPath;
        }

        [SecurityCritical]
        void CommonInit()
        {
            Contract.Assert(searchCriteria != null && searchData != null, "searchCriteria and searchData should be initialized");

            // Execute searchCriteria against the current directory 
            String searchPath = searchData.fullPath + searchCriteria;

            var data = new FindData();

            // Open a Find handle
            safeFindHandle = Win32Api.IO.FindFirstFile(searchPath, data);

            if( safeFindHandle.IsInvalid )
            {
                int hr = Marshal.GetLastWin32Error();
                if( hr != Win32Error.ERROR_FILE_NOT_FOUND && hr != Win32Error.ERROR_NO_MORE_FILES )
                {
                    HandleError(hr, searchData.fullPath);
                }
                else
                {
                    // flag this as empty only if we're searching just top directory 
                    // Used in fast path for top directory only 
                    isEmpty = searchData.searchOptions == SearchOption.TopDirectoryOnly;
                }
            }
            // fast path for TopDirectoryOnly. If we have a result, go ahead and set it to
            // current. If empty, dispose handle.
            if( searchData.searchOptions == SearchOption.TopDirectoryOnly )
            {
                if( isEmpty )
                {
                    safeFindHandle.Dispose();
                }
                else
                {
                    SearchResult searchResult = CreateSearchResult(searchData, data);
                    if( resultHandler.IsResultIncluded(searchResult) )
                    {
                        current = resultHandler.CreateObject(searchResult);
                    }
                }
            } 
                    // for AllDirectories, we first recurse into dirs, so cleanup and add searchData
                    // to the stack
            else
            {
                safeFindHandle.Dispose();
                searchStack.Add(searchData);
            }
        }

        [SecuritySafeCritical]
        protected override Iterator<TSource> Clone()
        {
            return new FileSystemEnumerator<TSource>(fullPath, normalizedSearchPath, searchCriteria, userPath, searchOption, resultHandler);
        }

        [SecuritySafeCritical]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if( safeFindHandle != null )
                {
                    safeFindHandle.Dispose();
                }
            }
            finally
            {
                Win32Api.SetErrorMode(oldMode);
                base.Dispose(disposing);
            }
        }

        [SecuritySafeCritical]
        public override bool MoveNext()
        {
            var data = new FindData();
            switch( state )
            {
                case stateInit:
                {
                    if( isEmpty )
                    {
                        state = stateFinish;
                        goto case stateFinish;
                    }
                    if( searchData.searchOptions == SearchOption.TopDirectoryOnly )
                    {
                        state = stateFindNextFile;
                        if( current != null )
                        {
                            return true;
                        }
                        goto case stateFindNextFile;
                    }
                    state = stateSearchNextDir;
                    goto case stateSearchNextDir;
                }
                case stateSearchNextDir:
                {
                    Contract.Assert(searchData.searchOptions != SearchOption.TopDirectoryOnly, "should not reach this code path if searchOption == TopDirectoryOnly");
                    // Traverse directory structure. We need to get '*'
                    while( searchStack.Count > 0 )
                    {
                        searchData = searchStack[0];
                        Contract.Assert((searchData.fullPath != null), "fullpath can't be null!");
                        searchStack.RemoveAt(0);

                        // Traverse the subdirs
                        AddSearchableDirsToStack(searchData);

                        // Execute searchCriteria against the current directory
                        String searchPath = searchData.fullPath + searchCriteria;

                        // Open a Find handle
                        safeFindHandle = Win32Api.IO.FindFirstFile(searchPath, data);
                        if( safeFindHandle.IsInvalid )
                        {
                            int hr = Marshal.GetLastWin32Error();
                            if( hr == Win32Error.ERROR_ACCESS_DENIED || hr == Win32Error.ERROR_FILE_NOT_FOUND || hr == Win32Error.ERROR_NO_MORE_FILES || hr == Win32Error.ERROR_PATH_NOT_FOUND )
                                continue;

                            safeFindHandle.Dispose();
                            HandleError(hr, searchData.fullPath);
                        }

                        state = stateFindNextFile;
                        needsParentPathDiscoveryDemand = true;
                        SearchResult searchResult = CreateSearchResult(searchData, data);
                        if( resultHandler.IsResultIncluded(searchResult) )
                        {
                            if( needsParentPathDiscoveryDemand )
                            {
                                DoDemand(searchData.fullPath);
                                needsParentPathDiscoveryDemand = false;
                            }
                            current = resultHandler.CreateObject(searchResult);
                            return true;
                        }
                        goto case stateFindNextFile;
                    }
                    state = stateFinish;
                    goto case stateFinish;
                }
                case stateFindNextFile:
                {
                    if( searchData != null && safeFindHandle != null )
                    {
                        // Keep asking for more matching files/dirs, add it to the list 
                        while( Win32Api.IO.FindNextFile(safeFindHandle, data) )
                        {
                            SearchResult searchResult = CreateSearchResult(searchData, data);
                            if( resultHandler.IsResultIncluded(searchResult) )
                            {
                                if( needsParentPathDiscoveryDemand )
                                {
                                    DoDemand(searchData.fullPath);
                                    needsParentPathDiscoveryDemand = false;
                                }
                                current = resultHandler.CreateObject(searchResult);
                                return true;
                            }
                        }

                        // Make sure we quit with a sensible error. 
                        int hr = Marshal.GetLastWin32Error();

                        if( safeFindHandle != null )
                            safeFindHandle.Dispose();

                        // ERROR_FILE_NOT_FOUND is valid here because if the top level
                        // dir doen't contain any subdirs and matching files then
                        // we will get here with this errorcode from the searchStack walk
                        if( (hr != 0) && (hr != Win32Error.ERROR_NO_MORE_FILES)
                            && (hr != Win32Error.ERROR_FILE_NOT_FOUND) )
                        {
                            HandleError(hr, searchData.fullPath);
                        }
                    }
                    if( searchData.searchOptions == SearchOption.TopDirectoryOnly )
                    {
                        state = stateFinish;
                        goto case stateFinish;
                    }
                    state = stateSearchNextDir;
                    goto case stateSearchNextDir;
                }
                case stateFinish:
                {
                    Dispose();
                    break;
                }
            }
            return false;
        }

        [SecurityCritical]
        SearchResult CreateSearchResult(SearchData localSearchData, FindData findData)
        {
            String userPathFinal = PathHelperMethods.InternalCombine(localSearchData.userPath, findData.FileName);
            String fullPathFinal = PathHelperMethods.InternalCombine(localSearchData.fullPath, findData.FileName);
            return new SearchResult(fullPathFinal, userPathFinal, findData);
        }

        [SecurityCritical]
        void HandleError(int hr, String path)
        {
            Dispose();
            ErrorHelper.WinIoError(hr, path);
        }

        [SecurityCritical] // auto-generated 
        void AddSearchableDirsToStack(SearchData localSearchData)
        {
            Contract.Requires(localSearchData != null);

            String searchPath = localSearchData.fullPath + "*";
            SafeFindHandle hnd = null;
            var data = new FindData();
            try
            {
                // Get all files and dirs
                hnd = Win32Api.IO.FindFirstFile(searchPath, data);

                if( hnd.IsInvalid )
                {
                    int hr = Marshal.GetLastWin32Error();

                    // This could happen if the dir doesn't contain any files.
                    // Continue with the recursive search though, eventually 
                    // searchStack will become empty
                    if( hr == Win32Error.ERROR_ACCESS_DENIED || hr == Win32Error.ERROR_FILE_NOT_FOUND || hr == Win32Error.ERROR_NO_MORE_FILES || hr == Win32Error.ERROR_PATH_NOT_FOUND )
                        return;

                    HandleError(hr, localSearchData.fullPath);
                }

                // Add subdirs to searchStack. Exempt ReparsePoints as appropriate
                int incr = 0;
                do
                {
                    if( data.IsDir )
                    {
                        // FullPath
                        var pathBuffer = new StringBuilder(localSearchData.fullPath);
                        pathBuffer.Append(data.FileName);
                        String tempFullPath = pathBuffer.ToString();

                        // UserPath
                        pathBuffer.Length = 0;
                        pathBuffer.Append(localSearchData.userPath);
                        pathBuffer.Append(data.FileName);

                        SearchOption option = localSearchData.searchOptions;

#if EXCLUDE_REPARSEPOINTS
        // Traverse reparse points depending on the searchoption specified 
                        if ((searchDataSubDir.searchOption == SearchOption.AllDirectories) && (0 != (data.dwFileAttributes & Win32Native.FILE_ATTRIBUTE_REPARSE_POINT)))
                            option = SearchOption.TopDirectoryOnly;
#endif
                        // Setup search data for the sub directory and push it into the stack 
                        var searchDataSubDir = new SearchData(tempFullPath, pathBuffer.ToString(), option);

                        searchStack.Insert(incr++, searchDataSubDir);
                    }
                } while( Win32Api.IO.FindNextFile(hnd, data) );
                // We don't care about errors here
            }
            finally
            {
                if( hnd != null )
                    hnd.Dispose();
            }
        }

        [SecurityCritical]
        internal static void DoDemand(String fullPath)
        {
            var demandPaths = new[] {GetDemandDir(fullPath, true)};
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, demandPaths).Demand();
        }

        static String NormalizeSearchPattern(String searchPattern)
        {
            Contract.Requires(searchPattern != null);

            // Win32 normalization trims only U+0020.
            String tempSearchPattern = searchPattern.TrimEnd(PathHelperMethods.TrimEndChars);

            // Make this corner case more useful, like dir 
            if( tempSearchPattern.Equals(".") )
            {
                tempSearchPattern = "*";
            }

            PathHelperMethods.CheckSearchPattern(tempSearchPattern);
            return tempSearchPattern;
        }

        static String GetNormalizedSearchCriteria(String fullSearchString, String fullPathMod)
        {
            Contract.Requires(fullSearchString != null);
            Contract.Requires(fullPathMod != null);
            Contract.Requires(fullSearchString.Length >= fullPathMod.Length);

            String searchCriteria = null;
            char lastChar = fullPathMod[fullPathMod.Length - 1];
            if( PathHelperMethods.IsDirectorySeparator(lastChar) )
            {
                // Can happen if the path is C:\temp, in which case GetDirectoryName would return C:\
                searchCriteria = fullSearchString.Substring(fullPathMod.Length);
            }
            else
            {
                Contract.Assert(fullSearchString.Length > fullPathMod.Length);
                searchCriteria = fullSearchString.Substring(fullPathMod.Length + 1);
            }
            return searchCriteria;
        }

        static String GetFullSearchString(String fullPath, String searchPattern)
        {
            Contract.Requires(fullPath != null);
            Contract.Requires(searchPattern != null);

            String tempStr = PathHelperMethods.InternalCombine(fullPath, searchPattern);

            // If path ends in a trailing slash (\), append a * or we'll get a "Cannot find the file specified" exception
            char lastChar = tempStr[tempStr.Length - 1];
            if( PathHelperMethods.IsDirectorySeparator(lastChar) || lastChar == Path.VolumeSeparatorChar )
            {
                tempStr = tempStr + '*';
            }

            return tempStr;
        }
    }
}