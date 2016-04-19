namespace IPFilter.Setup.CustomActions.IO
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Text;

    // ABOUT:
    // Helps with path normalization; support allocating on the stack or heap 
    //
    // PathHelper can't stackalloc the array for obvious reasons; you must pass 
    // in an array of chars allocated on the stack. 
    //
    // USAGE: 
    // Suppose you need to represent a char array of length len. Then this is the
    // suggested way to instantiate PathHelper:
    // ****************************************************************************
    // PathHelper pathHelper; 
    // if (charArrayLength less than stack alloc threshold == PathHelperMethods.MaxPath)
    //     char* arrayPtr = stackalloc char[PathHelperMethods.MaxPath]; 
    //     pathHelper = new PathHelper(arrayPtr); 
    // else
    //     pathHelper = new PathHelper(capacity, maxPath); 
    // ***************************************************************************
    //
    // note in the StringBuilder ctor:
    // - maxPath may be greater than PathHelperMethods.MaxPath (for isolated storage) 
    // - capacity may be greater than maxPath. This is even used for non-isolated
    //   storage scenarios where we want to temporarily allow strings greater 
    //   than PathHelperMethods.MaxPath if they can be normalized down to PathHelperMethods.MaxPath. This 
    //   can happen if the path contains escape characters "..".
    // 
    unsafe class PathHelper
    {
        // should not be serialized

        // maximum size, max be greater than max path if contains escape sequence
        readonly char* arrayPointer;
        readonly int capacity;
        // current length (next character position)
        // max path, may be less than capacity 
        readonly int maxPath;

        // ptr to stack alloc'd array of chars

        // whether to operate on stack alloc'd or heap alloc'd array 
        readonly bool useStackAlloc;
        int length;
        StringBuilder sb;

        // Instantiates a PathHelper with a stack alloc'd array of chars
        [SecurityCritical]
        internal PathHelper(char* charArrayPointer, int length)
        {
            Contract.Requires(charArrayPointer != null);
            Contract.Requires(length == PathHelperMethods.MaxPath);

            arrayPointer = charArrayPointer;
            capacity = length;
            maxPath = PathHelperMethods.MaxPath;
            useStackAlloc = true;
        }

        // Instantiates a PathHelper with a heap alloc'd array of ints. Will create a StringBuilder
        internal PathHelper(int capacity, int maxPath)
        {
            sb = new StringBuilder(capacity);
            this.capacity = capacity;
            this.maxPath = maxPath;
        }

        internal int Length
        {
            get
            {
                return useStackAlloc ? length : sb.Length;
            }
            set
            {
                if( useStackAlloc )
                {
                    length = value;
                }
                else
                {
                    sb.Length = value;
                }
            }
        }

        internal int Capacity
        {
            get { return capacity; }
        }

        internal char this[int index]
        {
            [SecurityCritical]
            get
            {
                Contract.Requires(index >= 0 && index < Length);
                return useStackAlloc ? arrayPointer[index] : sb[index];
            }
            [SecurityCritical]
            set
            {
                Contract.Requires(index >= 0 && index < Length);
                if( useStackAlloc )
                {
                    arrayPointer[index] = value;
                }
                else
                {
                    sb[index] = value;
                }
            }
        }

        [SecurityCritical]
        internal void Append(char value)
        {
            if( Length + 1 >= capacity )
                throw new PathTooLongException();

            if( useStackAlloc )
            {
                arrayPointer[Length] = value;
                length++;
            }
            else
            {
                sb.Append(value);
            }
        }

        [SecurityCritical] // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static void MemCopy(byte* src, int srcIndex, byte[] dest, int destIndex, int len)
        {
            Contract.Assert((srcIndex >= 0) && (destIndex >= 0) && (len >= 0), "Index and length must be non-negative!");
            Contract.Assert(dest.Length - destIndex >= len, "not enough bytes in dest");
            // If dest has 0 elements, the fixed statement will throw an 
            // IndexOutOfRangeException.  Special-case 0-byte copies.
            if( len == 0 )
                return;
            fixed( byte* pDest = dest )
            {
                MemCopyImpl(src + srcIndex, pDest + destIndex, len);
            }
        }

        [SecuritySafeCritical] // auto-generated
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static void MemCopy(byte[] src, int srcIndex, byte* pDest, int destIndex, int len)
        {
            Contract.Assert((srcIndex >= 0) && (destIndex >= 0) && (len >= 0), "Index and length must be non-negative!");
            Contract.Assert(src.Length - srcIndex >= len, "not enough bytes in src");
            // If dest has 0 elements, the fixed statement will throw an 
            // IndexOutOfRangeException.  Special-case 0-byte copies. 
            if( len == 0 ) return;
            fixed( byte* pSrc = src )
            {
                MemCopyImpl(pSrc + srcIndex, pDest + destIndex, len);
            }
        }

        [SecurityCritical] // auto-generated 
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static void MemCopy(char* pSrc, int srcIndex, char* pDest, int destIndex, int len)
        {
            Contract.Assert((srcIndex >= 0) && (destIndex >= 0) && (len >= 0), "Index and length must be non-negative!");

            // No boundary check for buffer overruns - dangerous
            if( len == 0 )
                return;
            MemCopyImpl((byte*)(pSrc + srcIndex), (byte*)(pDest + destIndex), len * 2);
        }

        // Note - using a long instead of an int for the length parameter
        // slows this method down by ~18%.
        [SecurityCritical] // auto-generated 
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static void MemCopyImpl(byte* src, byte* dest, int len)
        {
            Contract.Assert(len >= 0, "Negative length in memcopy!");

            // It turns out that on AMD64 it is faster to not be careful of alignment issues.
            // On IA64 it is necessary to be careful... Oh well. When we do the IA64 push we 
            // can work on this implementation.
#if IA64 
            long dstAlign = 8 - (((long)dest) & 7); // number of bytes to copy before dest is 8-byte aligned 

            while ((dstAlign > 0) && (len > 0)) 
            {
                *dest++ = *src++;

                len--; 
                dstAlign--;
            } 
 
            long srcAlign = 8 - (((long)src) & 7);
 
            if (len > 0)
            {
                if (srcAlign != 8)
                { 
                    if (4 == srcAlign)
                    { 
                        while (len >= 4) 
                        {
                            ((int*)dest)[0] = ((int*)src)[0]; 
                            dest += 4;
                            src  += 4;
                            len  -= 4;
                        } 

                        srcAlign = 2;   // fall through to 2-byte copies 
                    } 

                    if ((2 == srcAlign) || (6 == srcAlign)) 
                    {
                        while (len >= 2)
                        {
                            ((short*)dest)[0] = ((short*)src)[0]; 
                            dest += 2;
                            src  += 2; 
                            len  -= 2; 
                        }
                    } 

                    while (len-- > 0)
                    {
                        *dest++ = *src++; 
                    }
                } 
                else 
                {
                    if (len >= 16) 
                    {
                        do
                        {
                            ((long*)dest)[0] = ((long*)src)[0]; 
                            ((long*)dest)[1] = ((long*)src)[1];
                            dest += 16; 
                            src += 16; 
                        } while ((len -= 16) >= 16);
                    } 
                    if (len > 0)  // protection against negative len and optimization for len==16*N
                    {
                       if ((len & 8) != 0)
                       { 
                           ((long*)dest)[0] = ((long*)src)[0];
                           dest += 8; 
                           src += 8; 
                       }
                       if ((len & 4) != 0) 
                       {
                           ((int*)dest)[0] = ((int*)src)[0];
                           dest += 4;
                           src += 4; 
                       }
                       if ((len & 2) != 0) 
                       { 
                           ((short*)dest)[0] = ((short*)src)[0];
                           dest += 2; 
                           src += 2;
                       }
                       if ((len & 1) != 0)
                       { 
                           *dest++ = *src++;
                       } 
                    } 
                }
            }

#else
            // AMD64 implementation uses longs instead of ints where possible
            // 
            // <STRIP>This is a faster memcpy implementation, from
            // COMString.cpp.  For our strings, this beat the processor's 
            // repeat & move single byte instruction, which memcpy expands into. 
            // (You read that correctly.)
            // This is 3x faster than a simple while loop copying byte by byte, 
            // for large copies.</STRIP>
            if( len >= 16 )
            {
                do
                {
#if AMD64 
                    ((long*)dest)[0] = ((long*)src)[0]; 
                    ((long*)dest)[1] = ((long*)src)[1];
#else
                    ((int*)dest)[0] = ((int*)src)[0];
                    ((int*)dest)[1] = ((int*)src)[1];
                    ((int*)dest)[2] = ((int*)src)[2];
                    ((int*)dest)[3] = ((int*)src)[3];
#endif
                    dest += 16;
                    src += 16;
                } while( (len -= 16) >= 16 );
            }
            if( len > 0 ) // protection against negative len and optimization for len==16*N
            {
                if( (len & 8) != 0 )
                {
#if AMD64
                    ((long*)dest)[0] = ((long*)src)[0];
#else
                    ((int*)dest)[0] = ((int*)src)[0];
                    ((int*)dest)[1] = ((int*)src)[1];
#endif
                    dest += 8;
                    src += 8;
                }
                if( (len & 4) != 0 )
                {
                    ((int*)dest)[0] = ((int*)src)[0];
                    dest += 4;
                    src += 4;
                }
                if( (len & 2) != 0 )
                {
                    ((short*)dest)[0] = ((short*)src)[0];
                    dest += 2;
                    src += 2;
                }
                if( (len & 1) != 0 )
                    *dest++ = *src++;
            }

#endif
            // IA64
        }

        [SecurityCritical]
        internal int GetFullPathName()
        {
            if( useStackAlloc )
            {
                char* finalBuffer = stackalloc char[PathHelperMethods.MaxPath + 1];
                int result = Win32Api.GetFullPathName(arrayPointer, PathHelperMethods.MaxPath + 1, finalBuffer, IntPtr.Zero);

                // If success, the return buffer length does not account for the terminating null character.
                // If in-sufficient buffer, the return buffer length does account for the path + the terminating null character. 
                // If failure, the return buffer length is zero
                if( result > PathHelperMethods.MaxPath )
                {
                    char* tempBuffer = stackalloc char[result];
                    finalBuffer = tempBuffer;
                    result = Win32Api.GetFullPathName(arrayPointer, result, finalBuffer, IntPtr.Zero);
                }

                // Full path is genuinely long
                if( result >= PathHelperMethods.MaxPath )
                    throw new PathTooLongException();

                Contract.Assert(result < PathHelperMethods.MaxPath, "did we accidently remove a PathTooLongException check?");
                if( result == 0 && arrayPointer[0] != '\0' )
                {
                    ErrorHelper.WinIoError();
                }

                else if( result < PathHelperMethods.MaxPath )
                {
                    // Null terminate explicitly (may be only needed for some cases such as empty strings) 
                    // GetFullPathName return length doesn't account for null terminating char...
                    finalBuffer[result] = '\0'; // Safe to write directly as result is < PathHelperMethods.MaxPath
                }

                MemCopy(finalBuffer, 0, arrayPointer, 0, result);
                // Doesn't account for null terminating char. Think of this as the last 
                // valid index into the buffer but not the length of the buffer 
                Length = result;
                return result;
            }
            else
            {
                var finalBuffer = new StringBuilder(capacity + 1);
                int result = Win32Api.GetFullPathName(sb.ToString(), capacity + 1, finalBuffer, IntPtr.Zero);

                // If success, the return buffer length does not account for the terminating null character. 
                // If in-sufficient buffer, the return buffer length does account for the path + the terminating null character. 
                // If failure, the return buffer length is zero
                if( result > maxPath )
                {
                    finalBuffer.Length = result;
                    result = Win32Api.GetFullPathName(sb.ToString(), result, finalBuffer, IntPtr.Zero);
                }

                // Fullpath is genuinely long
                if( result >= maxPath )
                    throw new PathTooLongException();

                Contract.Assert(result < maxPath, "did we accidentally remove a PathTooLongException check?");
                if( result == 0 && sb[0] != '\0' )
                {
                    if( Length >= maxPath )
                    {
                        throw new PathTooLongException();
                    }
                    ErrorHelper.WinIoError();
                }
                sb = finalBuffer;
                return result;
            }
        }

        [SecurityCritical]
        internal bool TryExpandShortFileName()
        {
            if( useStackAlloc )
            {
                NullTerminate();
                char* buffer = UnsafeGetArrayPtr();
                char* shortFileNameBuffer = stackalloc char[PathHelperMethods.MaxPath + 1];

                int r = Win32Api.IO.GetLongPathName(buffer, shortFileNameBuffer, PathHelperMethods.MaxPath);

                // If success, the return buffer length does not account for the terminating null character.
                // If in-sufficient buffer, the return buffer length does account for the path + the terminating null character. 
                // If failure, the return buffer length is zero
                if( r >= PathHelperMethods.MaxPath )
                    throw new PathTooLongException();

                if( r == 0 )
                {
                    // Note: GetLongPathName will return ERROR_INVALID_FUNCTION on a
                    // path like \\.\PHYSICALDEVICE0 - some device driver doesn't
                    // support GetLongPathName on that string.  This behavior is
                    // by design, according to the Core File Services team. 
                    // We also get ERROR_NOT_ENOUGH_QUOTA in SQL_CLR_STRESS runs
                    // intermittently on paths like D:\DOCUME~1\user\LOCALS~1\Temp\ 
                    return false;
                }

                // Safe to copy as we have already done PathHelperMethods.MaxPath bound checking
                MemCopy(shortFileNameBuffer, 0, buffer, 0, r);
                Length = r;
                // We should explicitly null terminate as in some cases the long version of the path 
                // might actually be shorter than what we started with because of Win32's normalization
                // Safe to write directly as bufferLength is guaranteed to be < PathHelperMethods.MaxPath 
                NullTerminate();
                return true;
            }
            else
            {
                StringBuilder sb = GetStringBuilder();

                String origName = sb.ToString();
                String tempName = origName;
                bool addedPrefix = false;
                if( tempName.Length > PathHelperMethods.MaxPath )
                {
                    tempName = PathHelperMethods.AddLongPathPrefix(tempName);
                    addedPrefix = true;
                }
                sb.Capacity = capacity;
                sb.Length = 0;
                int r = Win32Api.IO.GetLongPathName(tempName, sb, capacity);

                if( r == 0 )
                {
                    // Note: GetLongPathName will return ERROR_INVALID_FUNCTION on a 
                    // path like \\.\PHYSICALDEVICE0 - some device driver doesn't
                    // support GetLongPathName on that string.  This behavior is 
                    // by design, according to the Core File Services team.
                    // We also get ERROR_NOT_ENOUGH_QUOTA in SQL_CLR_STRESS runs
                    // intermittently on paths like D:\DOCUME~1\user\LOCALS~1\Temp\
                    sb.Length = 0;
                    sb.Append(origName);
                    return false;
                }

                if( addedPrefix )
                    r -= 4;

                // If success, the return buffer length does not account for the terminating null character.
                // If in-sufficient buffer, the return buffer length does account for the path + the terminating null character. 
                // If failure, the return buffer length is zero
                if( r >= maxPath )
                    throw new PathTooLongException();

                sb = PathHelperMethods.RemoveLongPathPrefix(sb);
                Length = sb.Length;
                return true;
            }
        }

        [SecurityCritical]
        internal void Fixup(int lenSavedName, int lastSlash)
        {
            if( useStackAlloc )
            {
                char* savedName = stackalloc char[lenSavedName];
                MemCopy(arrayPointer, lastSlash + 1, savedName, 0, lenSavedName);
                Length = lastSlash;
                NullTerminate();
                bool r = TryExpandShortFileName();
                // Clean up changes made to the newBuffer. 
                Append(Path.DirectorySeparatorChar);
                if( Length + lenSavedName >= PathHelperMethods.MaxPath )
                    throw new PathTooLongException();
                MemCopy(savedName, 0, arrayPointer, Length, lenSavedName);
                Length = Length + lenSavedName;
            }
            else
            {
                String savedName = sb.ToString(lastSlash + 1, lenSavedName);
                Length = lastSlash;
                bool r = TryExpandShortFileName();
                // Clean up changes made to the newBuffer.
                Append(Path.DirectorySeparatorChar);
                if( Length + lenSavedName >= maxPath )
                    throw new PathTooLongException();
                sb.Append(savedName);
            }
        }

        [SecurityCritical]
        internal bool OrdinalStartsWith(String compareTo, bool ignoreCase)
        {
            if( Length < compareTo.Length )
                return false;

            if( useStackAlloc )
            {
                NullTerminate();
                if( ignoreCase )
                {
                    var s = new String(arrayPointer, 0, compareTo.Length);
                    return compareTo.Equals(s, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    for( int i = 0; i < compareTo.Length; i++ )
                    {
                        if( arrayPointer[i] != compareTo[i] )
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            else
            {
                if( ignoreCase )
                {
                    return sb.ToString().StartsWith(compareTo, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    return sb.ToString().StartsWith(compareTo, StringComparison.Ordinal);
                }
            }
        }

        [SecuritySafeCritical]
        public override String ToString()
        {
            if( useStackAlloc )
            {
                return new String(arrayPointer, 0, Length);
            }
            else
            {
                return sb.ToString();
            }
        }

        [SecurityCritical]
        char* UnsafeGetArrayPtr()
        {
            Contract.Assert(useStackAlloc, "This should never be called for PathHelpers wrapping a StringBuilder");
            return arrayPointer;
        }

        StringBuilder GetStringBuilder()
        {
            Contract.Assert(!useStackAlloc, "This should never be called for PathHelpers that wrap a stackalloc'd buffer");
            return sb;
        }

        [SecurityCritical]
        void NullTerminate()
        {
            Contract.Assert(useStackAlloc, "This should never be called for PathHelpers wrapping a StringBuilder");
            arrayPointer[length] = '\0';
        }
    }
}