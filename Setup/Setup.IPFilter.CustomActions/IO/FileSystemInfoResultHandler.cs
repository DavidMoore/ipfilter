namespace IPFilter.Setup.CustomActions.IO
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    public class FileSystemInfoResultHandler : SearchResultHandler<FileSystemInfo>
    {
        [SecurityCritical]
        internal override bool IsResultIncluded(SearchResult result)
        {
            if (result == null) throw new ArgumentNullException("result");
            bool includeFile = result.FindData.IsFile;
            bool includeDir = result.FindData.IsDir;
            Contract.Assert(!(includeFile && includeDir), result.FindData.FileName + ": current item can't be both file and dir!");
    
            return (includeDir || includeFile);
        }
    
        [SecurityCritical]
        internal override FileSystemInfo CreateObject(SearchResult result)
        {
            if (result == null) throw new ArgumentNullException("result");

            bool isFile = result.FindData.IsFile;
            bool isDir = result.FindData.IsDir;
    
            if (isDir)
            {
                string name = result.FullPath;
                string permissionName = name + "\\.";
                string[] permissionNames = new [] { permissionName };
                new FileIOPermission(FileIOPermissionAccess.Read, permissionNames).Demand();
                // TODO: Find way to prevent security demand for performance
                var di = new DirectoryInfo(name);
                //di.InitializeFrom(result.FindData);
                return di;
            }
            else
            {
                Contract.Assert(isFile);
                string name = result.FullPath;
                string[] names = new [] { name };
                new FileIOPermission(FileIOPermissionAccess.Read, names).Demand();
                // TODO: Find way to prevent security demand for performance
                var fi = new FileInfo(name);
                //fi.InitializeFrom(result.FindData);
                return fi;
            }
        }
    }
}