namespace IPFilter.ListProviders
{
    using System;
    using System.Collections.Generic;
    using Models;

    public class EmuleSecurity : IMirrorProvider
    {
        static readonly IEnumerable<FileMirror> mirrors = new[] { new FileMirror("Default", "Emule Security") };

        /// <summary>
        /// Gets a list of mirrors for this provider
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileMirror> GetMirrors()
        {
            return mirrors;
        }

        /// <summary>
        /// The name of the mirror provider
        /// </summary>
        public string Name { get { return "Emule Security"; } }

        public string GetUrlForMirror(FileMirror mirror)
        {
            if(string.Equals("Default", mirror.Id, StringComparison.OrdinalIgnoreCase))
            {
                return "http://upd.emule-security.org/ipfilter.zip";
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}