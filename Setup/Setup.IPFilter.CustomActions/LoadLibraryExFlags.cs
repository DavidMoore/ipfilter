namespace IPFilter.Setup.CustomActions
{
    using System;

    /// <summary>
    /// Actions to be taken when loading a module using <see cref="Win32Api.LoadLibraryEx"/>.
    /// </summary>
    [Flags]
    public enum LoadLibraryExFlags
    {
        /// <summary>
        /// No options.
        /// </summary>
        None = 0,

        /// <summary>
        /// If this value is used, and the executable module is a DLL, the system does
        /// not call DllMain for process and thread initialization and termination.
        /// Also, the system does not load additional executable modules that are referenced by the specified module.
        /// </summary>
        /// <remarks>
        /// Do not use this value; it is provided only for backwards compatibility.
        /// If you are planning to access only data or resources in the DLL,
        /// use <see cref="LoadLibraryAsDatafileExclusive"/> or <see cref="LoadLibraryAsImageResource"/> or both.
        /// Otherwise, load the library as a DLL or executable module using the LoadLibrary function.</remarks>
        DoNotResolveDllReferences = 0x00000001,

        /// <summary>
        /// If this value is used, the system does not check AppLocker rules or apply Software Restriction Policies for the DLL
        /// This action applies only to the DLL being loaded and not to its dependents. This value is recommended for use in
        /// setup programs that must run extracted DLLs during installation.
        /// </summary>
        /// <remarks>
        /// Windows Server 2008, Windows Vista, Windows Server 2003, and Windows XP:  AppLocker was introduced in Windows 7 and Windows Server 2008 R2.
        /// Windows 2000:  This value is not supported until Windows XP.
        /// </remarks>
        LoadIgnoreCodeAuthzLevel = 0x00000010,


        /// <summary>
        /// If this value is used, the system maps the file into the calling process's virtual address space as if it were
        /// a data file. Nothing is done to execute or prepare to execute the mapped file. Therefore, you cannot call functions
        /// like GetModuleFileName, GetModuleHandle or GetProcAddress with this DLL. Using this value causes writes to read-only
        /// memory to raise an access violation. Use this flag when you want to load a DLL only to extract messages or resources from it.
        /// This value can be used with <see cref="LoadLibraryAsImageResource"/>. For more information, see Remarks.
        /// </summary>
        LoadLibraryAsDatafile = 0x00000002,

        /// <summary>
        /// Similar to <see cref="LoadLibraryAsDatafile"/>, except that the DLL file on the disk is opened for exclusive write access.
        /// Therefore, other processes cannot open the DLL file for write access while it is in use. However, the DLL can
        /// still be opened by other processes. This value can be used with <see cref="LoadLibraryAsImageResource"/>. For more information, see Remarks.
        /// </summary>
        /// <remarks>Windows Server 2003 and Windows XP/2000:  This value is not supported until Windows Vista.</remarks>
        LoadLibraryAsDatafileExclusive = 0x00000040,

        /// <summary>
        /// If this value is used, the system maps the file into the process's virtual address space as an image file.
        /// However, the loader does not load the static imports or perform the other usual initialization steps. Use this flag
        /// when you want to load a DLL only to extract messages or resources from it.
        /// Unless the application depends on the image layout, this value should be used with either <see cref="LoadLibraryAsDatafileExclusive"/>
        /// or <see cref="LoadLibraryAsDatafile"/>. For more information, see the Remarks section.
        /// </summary>
        /// <remarks>Windows Server 2003 and Windows XP/2000:  This value is not supported until Windows Vista.</remarks>
        LoadLibraryAsImageResource = 0x00000020,

        /// <summary>
        /// If this value is used and lpFileName specifies an absolute path, the system uses the alternate file search strategy
        /// discussed in the Remarks section to find associated executable modules that the specified module causes to be loaded.
        /// If this value is used and lpFileName specifies a relative path, the behavior is undefined.
        /// If this value is not used, or if lpFileName does not specify a path, the system uses the standard search strategy discussed
        /// in the Remarks section to find associated executable modules that the specified module causes to be loaded.
        /// </summary>
        LoadWithAlteredSearchPath = 0x00000008
    }
}