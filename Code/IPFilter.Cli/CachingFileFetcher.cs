using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using IPFilter.Core;

namespace IPFilter.Cli
{
    class CachingFileFetcher : FileFetcher
    {
        static readonly MD5 hasher = MD5.Create();

        readonly DirectoryInfo path;

        public CachingFileFetcher()
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"ipfilter\cache\");
            path = new DirectoryInfo(basePath );
            if (!path.Exists) path.Create();
        }

        public override async Task<FileNode> Get(Uri uri, FilterContext context)
        {
            // The path to the cached version of the file (which may or may not exist, depending on if it's
            // already been downloaded and cached before).
            var cachePath = GetCachePath(uri);

            if (uri.IsFile)
            {
                return await GetLocalFile(uri, context, cachePath);
            }

            return await GetRemoteFile(uri, context, cachePath);
        }
        
        FileInfo GetCachePath(Uri uri)
        {
            var hash = hasher.ComputeHash(uri);
            var cachePath = Path.Combine(path.FullName, hash);
            return new FileInfo(cachePath);
        }

        public override void Dispose()
        {
            base.Dispose();
            hasher?.Dispose();
        }
    }
}