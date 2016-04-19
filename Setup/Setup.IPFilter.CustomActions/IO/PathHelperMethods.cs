namespace IPFilter.Setup.CustomActions.IO
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Text;

    public static class PathHelperMethods
    {
        // Platform specific volume separator character.  This is colon (':')
        // on Windows and MacOS, and slash ('/') on Unix.  This is mostly 
        // useful for parsing paths like "c:\windows" or "MacVolume:System Folder".
        //
#if !PLATFORM_UNIX
        public const char VolumeSeparatorChar = ':';
#else
        public const char VolumeSeparatorChar = '/'; 
#endif // !PLATFORM_UNIX 

        internal static String InternalCombine(String path1, String path2)
        {
            if (path1 == null || path2 == null) throw new ArgumentNullException((path1 == null) ? "path1" : "path2");
            Contract.EndContractBlock();
            CheckInvalidPathChars(path1);
            CheckInvalidPathChars(path2);

            if (path2.Length == 0)
                throw new ArgumentException("Argument_PathEmpty", "path2");
            if (IsPathRooted(path2))
                throw new ArgumentException("Arg_Path2IsRooted", "path2");
            int i = path1.Length;
            if (i == 0) return path2;
            char ch = path1[i - 1];
            if (ch != DirectorySeparatorChar && ch != AltDirectorySeparatorChar && ch != VolumeSeparatorChar)
                return path1 + DirectorySeparatorChar + path2;
            return path1 + path2;
        }

        // Tests if the given path contains a root. A path is considered rooted
        // if it starts with a backslash ("\") or a drive letter and a colon (":"). 
        //
        public static bool IsPathRooted(String path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);

                int length = path.Length;
                if ((length >= 1 && (path[0] == DirectorySeparatorChar || path[0] == AltDirectorySeparatorChar))
#if !PLATFORM_UNIX
 || (length >= 2 && path[1] == VolumeSeparatorChar)
#endif
) return true;
            }
            return false;
        }

        internal unsafe static String RemoveLongPathPrefix(String path)
        {
            if (!path.StartsWith(Prefix, StringComparison.Ordinal))
                return path;
            else
                return path.Substring(4);
        }

        internal unsafe static StringBuilder RemoveLongPathPrefix(StringBuilder path)
        {
            if (!path.ToString().StartsWith(Prefix, StringComparison.Ordinal))
                return path;
            else
                return path.Remove(0, 4);
        } 

        private static readonly String Prefix = @"\\?\"; 

        internal unsafe static String AddLongPathPrefix(String path)
        {
            if (path.StartsWith(Prefix, StringComparison.Ordinal))
                return path;
            else
                return Prefix + path;
        }

        // Platform specific directory separator character.  This is backslash 
        // ('\') on Windows, slash ('/') on Unix, and colon (':') on Mac.
        // 
#if !PLATFORM_UNIX
        public static readonly char DirectorySeparatorChar = '\\';
#else
        public static readonly char DirectorySeparatorChar = '/'; 
#endif // !PLATFORM_UNIX

        // Platform specific alternate directory separator character. 
        // This is backslash ('\') on Unix, and slash ('/') on Windows
        // and MacOS. 
        //
#if !PLATFORM_UNIX
        public static readonly char AltDirectorySeparatorChar = '/';
#else 
        public static readonly char AltDirectorySeparatorChar = '\\';
#endif // !PLATFORM_UNIX 

        // Trim trailing white spaces, tabs etc but don't be aggressive in removing everything that has UnicodeCategory of trailing space.
        // String.WhitespaceChars will trim aggressively than what the underlying FS does (for ex, NTFS, FAT). 
        internal static readonly char[] TrimEndChars = { (char)0x9, (char)0xA, (char)0xB, (char)0xC, (char)0xD, (char)0x20, (char)0x85, (char)0xA0 };

        public static bool IsDirectorySeparator(char c)
        {
            return (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar);
        }

        // ".." can only be used if it is specified as a part of a valid File/Directory name. We disallow
        //  the user being able to use it to move up directories. Here are some examples eg 
        //    Valid: a..b  abc..d 
        //    Invalid: ..ab   ab..  ..   abc..d\abc..
        // 
        [System.Security.SecuritySafeCritical]  // auto-generated
        internal static void CheckSearchPattern(String searchPattern)
        {
            int index;
            while ((index = searchPattern.IndexOf("..", StringComparison.Ordinal)) != -1)
            {
                // Terminal ".." . Files names cannot end in ".."
                if (index + 2 == searchPattern.Length) throw new ArgumentException("Invalid search pattern: " + searchPattern, "searchPattern");

                if ((searchPattern[index + 2] == DirectorySeparatorChar)
                    || (searchPattern[index + 2] == AltDirectorySeparatorChar))
                    throw new ArgumentException("Invalid search pattern: " + searchPattern, "searchPattern");

                searchPattern = searchPattern.Substring(index + 2);
            }
        }

        // This method is package access to let us quickly get a string name
        // while avoiding a security check.  This also serves a slightly 
        // different purpose - when we open a file, we need to resolve the
        // path into a fully qualified, non-relative path name.  This
        // method does that, finding the current drive &; directory.  But
        // as long as we don't return this info to the user, we're good.  However, 
        // the public GetFullPath does need to do a security check.
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static String GetFullPathInternal(String path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            Contract.EndContractBlock();

            String newPath = NormalizePath(path, true);

            return newPath;
        }

        [System.Security.SecuritySafeCritical]  // auto-generated 
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal unsafe static String NormalizePath(String path, bool fullCheck)
        {
            return NormalizePath(path, fullCheck, MaxPath);
        }

        // Make this public sometime.
        // The max total path is 260, and the max individual component length is 255. 
        // For example, D:\<256 char file name> isn't legal, even though it's under 260 chars. 
        internal const int MaxPath = 260;
        internal const int MaxDirectoryLength = 255;

        internal static void CheckInvalidPathChars(String path)
        {
#if PLATFORM_UNIX
            if (path.Length >= 2 && path[0] == '\\' && path[1] == '\\') 
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPathChars"));
            Contract.EndContractBlock(); 
#endif // PLATFORM_UNIX

            for (int i = 0; i < path.Length; i++)
            {
                int c = path[i];

                // Note: This list is duplicated in static char[] InvalidPathChars 
                if (c == '\"' || c == '<' || c == '>' || c == '|' || c < 32)
                    throw new ArgumentException("Argument_InvalidPathChars");
            }
        }

        [System.Security.SecurityCritical]  // auto-generated 
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal unsafe static String NormalizePath(String path, bool fullCheck, int maxPathLength)
        {

            Contract.Requires(path != null, "path can't be null");
            // If we're doing a full path check, trim whitespace and look for 
            // illegal path characters.
            if (fullCheck)
            {
                // Trim whitespace off the end of the string. 
                // Win32 normalization trims only U+0020.
                path = path.TrimEnd(TrimEndChars);

                // Look for illegal path characters.
                CheckInvalidPathChars(path);
            }

            int index = 0;
            // We prefer to allocate on the stack for workingset/perf gain. If the 
            // starting path is less than MaxPath then we can stackalloc; otherwise we'll
            // use a StringBuilder (PathHelper does this under the hood). The latter may 
            // happen in 2 cases:
            // 1. Starting path is greater than MaxPath but it normalizes down to MaxPath.
            // This is relevant for paths containing escape sequences. In this case, we
            // attempt to normalize down to MaxPath, but the caller pays a perf penalty 
            // since StringBuilder is used.
            // 2. IsolatedStorage, which supports paths longer than MaxPath (value given 
            // by maxPathLength. 
            PathHelper newBuffer;
            if (path.Length <= MaxPath)
            {
                char* charArrayPointer = stackalloc char[MaxPath];
                newBuffer = new PathHelper(charArrayPointer, MaxPath);
            }
            else
            {
                newBuffer = new PathHelper(path.Length + MaxPath, maxPathLength);
            }

            uint numSpaces = 0;
            uint numDots = 0;
            bool fixupDirectorySeparator = false;
            // Number of significant chars other than potentially suppressible
            // dots and spaces since the last directory or volume separator char
            uint numSigChars = 0;
            int lastSigChar = -1; // Index of last significant character. 
            // Whether this segment of the path (not the complete path) started
            // with a volume separator char.  Reject "c:...". 
            bool startedWithVolumeSeparator = false;
            bool firstSegment = true;
            bool mightBeShortFileName = false;
            int lastDirectorySeparatorPos = 0;

#if !PLATFORM_UNIX
            // LEGACY: This code is here for backwards compatibility reasons. It 
            // ensures that \\foo.cs\bar.cs stays \\foo.cs\bar.cs instead of being
            // turned into \foo.cs\bar.cs. 
            if (path.Length > 0 && (path[0] == DirectorySeparatorChar || path[0] == AltDirectorySeparatorChar))
            {
                newBuffer.Append('\\');
                index++;
                lastSigChar = 0;
            }
#endif

            // Normalize the string, stripping out redundant dots, spaces, and
            // slashes. 
            while (index < path.Length)
            {
                char currentChar = path[index];

                // We handle both directory separators and dots specially.  For
                // directory separators, we consume consecutive appearances.
                // For dots, we consume all dots beyond the second in
                // succession.  All other characters are added as is.  In 
                // addition we consume all spaces after the last other char
                // in a directory name up until the directory separator. 

                if (currentChar == DirectorySeparatorChar || currentChar == AltDirectorySeparatorChar)
                {
                    // If we have a path like "123.../foo", remove the trailing dots. 
                    // However, if we found "c:\temp\..\bar" or "c:\temp\...\bar", don't.
                    // Also remove trailing spaces from both files & directory names.
                    // This was agreed on with the OS team to fix undeletable directory
                    // names ending in spaces. 

                    // If we saw a '\' as the previous last significant character and 
                    // are simply going to write out dots, suppress them. 
                    // If we only contain dots and slashes though, only allow
                    // a string like [dot]+ [space]*.  Ignore everything else. 
                    // Legal: "\.. \", "\...\", "\. \"
                    // Illegal: "\.. .\", "\. .\", "\ .\"
                    if (numSigChars == 0)
                    {
                        // Dot and space handling 
                        if (numDots > 0)
                        {
                            // Look for ".[space]*" or "..[space]*" 
                            int start = lastSigChar + 1;
                            if (path[start] != '.') throw new ArgumentException("Arg_PathIllegal");

                            // Only allow "[dot]+[space]*", and normalize the
                            // legal ones to "." or ".."
                            if (numDots >= 2)
                            {
                                // Reject "C:..."
                                if (startedWithVolumeSeparator && numDots > 2) throw new ArgumentException("Arg_PathIllegal");

                                if (path[start + 1] == '.')
                                {
                                    // Search for a space in the middle of the
                                    // dots and throw
                                    for (int i = start + 2; i < start + numDots; i++)
                                    {
                                        if (path[i] != '.') throw new ArgumentException("Arg_PathIllegal");
                                    }

                                    numDots = 2;
                                }
                                else
                                {
                                    if (numDots > 1) throw new ArgumentException("Arg_PathIllegal");
                                    numDots = 1;
                                }
                            }

                            if (numDots == 2)
                            {
                                newBuffer.Append('.');
                            }

                            newBuffer.Append('.');
                            fixupDirectorySeparator = false;

                            // Continue in this case, potentially writing out '\'. 
                        }

                        if (numSpaces > 0 && firstSegment)
                        {
                            // Handle strings like " \\server\share".
                            if (index + 1 < path.Length &&
                                (path[index + 1] == DirectorySeparatorChar || path[index + 1] == AltDirectorySeparatorChar))
                            {
                                newBuffer.Append(DirectorySeparatorChar);
                            }
                        }
                    }
                    numDots = 0;
                    numSpaces = 0;  // Suppress trailing spaces

                    if (!fixupDirectorySeparator)
                    {
                        fixupDirectorySeparator = true;
                        newBuffer.Append(DirectorySeparatorChar);
                    }
                    numSigChars = 0;
                    lastSigChar = index;
                    startedWithVolumeSeparator = false;
                    firstSegment = false;

#if !PLATFORM_UNIX
                    // For short file names, we must try to expand each of them as
                    // soon as possible.  We need to allow people to specify a file 
                    // name that doesn't exist using a path with short file names 
                    // in it, such as this for a temp file we're trying to create:
                    // C:\DOCUME~1\USERNA~1.RED\LOCALS~1\Temp\bg3ylpzp 
                    // We could try doing this afterwards piece by piece, but it's
                    // probably a lot simpler to do it here.
                    if (mightBeShortFileName)
                    {
                        newBuffer.TryExpandShortFileName();
                        mightBeShortFileName = false;
                    }
#endif
                    int thisPos = newBuffer.Length - 1;
                    if (thisPos - lastDirectorySeparatorPos > MaxDirectoryLength)
                    {
                        throw new PathTooLongException("IO.PathTooLong");
                    }
                    lastDirectorySeparatorPos = thisPos;
                } // if (Found directory separator)
                else if (currentChar == '.')
                {
                    // Reduce only multiple .'s only after slash to 2 dots. For 
                    // instance a...b is a valid file name.
                    numDots++;
                    // Don't flush out non-terminal spaces here, because they may in
                    // the end not be significant.  Turn "c:\ . .\foo" -> "c:\foo"
                    // which is the conclusion of removing trailing dots & spaces,
                    // as well as folding multiple '\' characters. 
                }
                else if (currentChar == ' ')
                {
                    numSpaces++;
                }
                else
                {  // Normal character logic 
#if !PLATFORM_UNIX
                    if (currentChar == '~')
                        mightBeShortFileName = true;
#endif

                    fixupDirectorySeparator = false;

#if !PLATFORM_UNIX
                    // To reject strings like "C:...\foo" and "C  :\foo" 
                    if (firstSegment && currentChar == VolumeSeparatorChar)
                    {
                        // Only accept "C:", not "c :" or ":"
                        // Get a drive letter or ' ' if index is 0.
                        char driveLetter = (index > 0) ? path[index - 1] : ' ';
                        bool validPath = ((numDots == 0) && (numSigChars >= 1) && (driveLetter != ' '));
                        if (!validPath) throw new ArgumentException("Arg_PathIllegal");

                        startedWithVolumeSeparator = true;
                        // We need special logic to make " c:" work, we should not fix paths like "  foo::$DATA"
                        if (numSigChars > 1)
                        { // Common case, simply do nothing
                            var spaceCount = 0; // How many spaces did we write out, numSpaces has already been reset.
                            while ((spaceCount < newBuffer.Length) && newBuffer[spaceCount] == ' ') spaceCount++;

                            if (numSigChars - spaceCount == 1)
                            {
                                //Safe to update stack ptr directly 
                                newBuffer.Length = 0;
                                newBuffer.Append(driveLetter); // Overwrite spaces, we need a special case to not break "  foo" as a relative path. 
                            }
                        }
                        numSigChars = 0;
                    }
                    else
#endif // !PLATFORM_UNIX
                    {
                        numSigChars += 1 + numDots + numSpaces;
                    }

                    // Copy any spaces & dots since the last significant character
                    // to here.  Note we only counted the number of dots & spaces,
                    // and don't know what order they're in.  Hence the copy. 
                    if (numDots > 0 || numSpaces > 0)
                    {
                        int numCharsToCopy = (lastSigChar >= 0) ? index - lastSigChar - 1 : index;
                        if (numCharsToCopy > 0)
                        {
                            for (int i = 0; i < numCharsToCopy; i++)
                            {
                                newBuffer.Append(path[lastSigChar + 1 + i]);
                            }
                        }
                        numDots = 0;
                        numSpaces = 0;
                    }

                    newBuffer.Append(currentChar);
                    lastSigChar = index;
                }

                index++;
            } // end while

            if (newBuffer.Length - 1 - lastDirectorySeparatorPos > MaxDirectoryLength)
            {
                throw new PathTooLongException();
            }

            // Drop any trailing dots and spaces from file & directory names, EXCEPT
            // we MUST make sure that "C:\foo\.." is correctly handled.
            // Also handle "C:\foo\." -> "C:\foo", while "C:\." -> "C:\"
            if (numSigChars == 0)
            {
                if (numDots > 0)
                {
                    // Look for ".[space]*" or "..[space]*" 
                    int start = lastSigChar + 1;
                    if (path[start] != '.') throw new ArgumentException("Arg_PathIllegal");

                    // Only allow "[dot]+[space]*", and normalize the
                    // legal ones to "." or ".."
                    if (numDots >= 2)
                    {
                        // Reject "C:..."
                        if (startedWithVolumeSeparator && numDots > 2) throw new ArgumentException("Arg_PathIllegal");

                        if (path[start + 1] == '.')
                        {
                            // Search for a space in the middle of the
                            // dots and throw
                            for (var i = start + 2; i < start + numDots; i++)
                            {
                                if (path[i] != '.') throw new ArgumentException("Arg_PathIllegal");
                            }

                            numDots = 2;
                        }
                        else
                        {
                            if (numDots > 1) throw new ArgumentException("Arg_PathIllegal");
                            numDots = 1;
                        }
                    }

                    if (numDots == 2)
                    {
                        newBuffer.Append('.');
                    }

                    newBuffer.Append('.');
                }
            } // if (numSigChars == 0)

            // If we ended up eating all the characters, bail out. 
            if (newBuffer.Length == 0) throw new ArgumentException("Arg_PathIllegal");

            // Disallow URL's here.  Some of our other Win32 API calls will reject
            // them later, so we might be better off rejecting them here.
            // Note we've probably turned them into "file:\D:\foo.tmp" by now. 
            // But for compatibility, ensure that callers that aren't doing a
            // full check aren't rejected here. 
            if (fullCheck)
            {
                if (newBuffer.OrdinalStartsWith("http:", false) || newBuffer.OrdinalStartsWith("file:", false))
                {
                    throw new ArgumentException("Argument_PathUriFormatNotSupported");
                }
            }

#if !PLATFORM_UNIX
            // If the last part of the path (file or directory name) had a tilde, 
            // expand that too.
            if (mightBeShortFileName)
            {
                newBuffer.TryExpandShortFileName();
            }
#endif

            // Call the Win32 API to do the final canonicalization step.
            var result = 1;

            if (fullCheck)
            {
                // NOTE: Win32 GetFullPathName requires the input buffer to be big enough to fit the initial 
                // path which is a concat of CWD and the relative path, this can be of an arbitrary
                // size and could be > MAX_PATH (which becomes an artificial limit at this point),
                // even though the final normalized path after fixing up the relative path syntax
                // might be well within the MAX_PATH restriction. For ex, 
                // "c:\SomeReallyLongDirName(thinkGreaterThan_MAXPATH)\..\foo.txt" which actually requires a
                // buffer well with in the MAX_PATH as the normalized path is just "c:\foo.txt" 
                // This buffer requirement seems wrong, it could be a bug or a perf optimization 
                // like returning required buffer length quickly or avoid stratch buffer etc.
                // Either way we need to workaround it here... 

                // Ideally we would get the required buffer length first by calling GetFullPathName
                // once without the buffer and use that in the later call but this doesn't always work
                // due to Win32 GetFullPathName bug. For instance, in Win2k, when the path we are trying to 
                // fully qualify is a single letter name (such as "a", "1", ",") GetFullPathName
                // fails to return the right buffer size (i.e, resulting in insufficient buffer). 
                // To workaround this bug we will start with MAX_PATH buffer and grow it once if the 
                // return value is > MAX_PATH.

                result = newBuffer.GetFullPathName();

#if !PLATFORM_UNIX
                // If we called GetFullPathName with something like "foo" and our 
                // command window was in short file name mode (ie, by running edlin or
                // DOS versions of grep, etc), we might have gotten back a short file 
                // name.  So, check to see if we need to expand it. 
                mightBeShortFileName = false;
                for (var i = 0; i < newBuffer.Length && !mightBeShortFileName; i++)
                {
                    if (newBuffer[i] == '~') mightBeShortFileName = true;
                }

                if (mightBeShortFileName)
                {
                    var r = newBuffer.TryExpandShortFileName();
                    // Consider how the path "Doesn'tExist" would expand.  If 
                    // we add in the current directory, it too will need to be
                    // fully expanded, which doesn't happen if we use a file 
                    // name that doesn't exist.
                    if (!r)
                    {
                        var lastSlash = -1;

                        for (var i = newBuffer.Length - 1; i >= 0; i--)
                        {
                            if( newBuffer[i] != DirectorySeparatorChar ) continue;

                            lastSlash = i;
                            break;
                        }

                        if (lastSlash >= 0)
                        {

                            // This bounds check is for safe memcpy but we should never get this far
                            if (newBuffer.Length >= maxPathLength)
                                throw new PathTooLongException("IO.PathTooLong");

                            int lenSavedName = newBuffer.Length - lastSlash - 1;
                            Contract.Assert(lastSlash < newBuffer.Length, "path unexpectedly ended in a '\'");

                            newBuffer.Fixup(lenSavedName, lastSlash);
                        }
                    }
                }
#endif
            }

            if (result != 0)
            {
                /* Throw an ArgumentException for paths like \\, \\server, \\server\
                   This check can only be properly done after normalizing, so
                   \\foo\.. will be properly rejected.  Also, reject \\?\GLOBALROOT\ 
                   (an internal kernel path) because it provides aliases for drives. */
                if (newBuffer.Length > 1 && newBuffer[0] == '\\' && newBuffer[1] == '\\')
                {
                    int startIndex = 2;
                    while (startIndex < result)
                    {
                        if (newBuffer[startIndex] == '\\')
                        {
                            startIndex++;
                            break;
                        }
                        startIndex++;
                    }
                    if (startIndex == result) throw new ArgumentException("Arg_PathIllegalUNC");

                    // Check for \\?\Globalroot, an internal mechanism to the kernel
                    // that provides aliases for drives and other undocumented stuff.
                    // The kernel team won't even describe the full set of what 
                    // is available here - we don't want managed apps mucking
                    // with this for security reasons. 
                    if (newBuffer.OrdinalStartsWith("\\\\?\\globalroot", true)) throw new ArgumentException("Arg_PathGlobalRoot");
                }
            }

            // Check our result and form the managed string as necessary.
            if (newBuffer.Length >= maxPathLength) throw new PathTooLongException("IO.PathTooLong");

            if (result == 0)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 0) errorCode = Win32Error.ERROR_BAD_PATHNAME;
                ErrorHelper.WinIoError(errorCode, path);
                return null;  // Unreachable - silence a compiler error.
            }

            var returnVal = newBuffer.ToString();
            if (string.Equals(returnVal, path, StringComparison.Ordinal))
            {
                returnVal = path;
            }
            return returnVal;
        }
    }
}