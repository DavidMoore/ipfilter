namespace IPFilter.ListProviders
{
    using System.Collections.Generic;
    using System.Globalization;
    using Models;

    public class BlocklistMirrorProvider : IMirrorProvider {
        /// <summary>
        /// Gets a list of mirrors for this provider
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileMirror> GetMirrors()
        {
            return new[] { new FileMirror("ipfilter.dat.gz", "Default") };
        }

        /// <summary>
        /// The name of the mirror provider
        /// </summary>
        public string Name { get { return "I-BlockList"; } }

        public string GetUrlForMirror(FileMirror mirror)
        {
            return string.Format(CultureInfo.CurrentCulture, "http://tbg.iblocklist.com/Lists/{0}", mirror.Id);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}