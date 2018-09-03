using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace IPFilter.Core
{
    /// <summary>
    /// Resolves and validates the URIs for various list sources.
    /// </summary>
    public class UriResolver : IUriResolver
    {
        public Uri Resolve(string url)
        {
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri uri)) return null;
            if (!uri.IsAbsoluteUri) return null;

            var builder = new UriBuilder(uri);
            
            switch (uri.Scheme)
            {
                case "http":
                case "https":
                    return uri;

                case "file":
                    return new Uri(Path.GetFullPath(uri.LocalPath));
                    
                default:
                    return null;
            }

            return null;
        }
    }
}
