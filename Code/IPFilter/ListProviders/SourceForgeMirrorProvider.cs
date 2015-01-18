namespace IPFilter.ListProviders
{
    using System.Collections.Generic;
    using System.Globalization;
    using Models;

    public class SourceForgeMirrorProvider : IMirrorProvider
    {
        readonly ISourceForgeMirrorListDownloader listDownloader;
        readonly SourceForgeMirrorParser parser;

        public const string DefaultMirrorListUrl = "https://sourceforge.net/settings/mirror_choices?projectname=emulepawcio&filename=Ipfilter/Ipfilter/ipfilter.zip";

        public SourceForgeMirrorProvider() : this( new SourceForgeMirrorParser(), DefaultMirrorListUrl ) {}

        public SourceForgeMirrorProvider(SourceForgeMirrorParser mirrorParser, string mirrorListUrl)
            : this(mirrorParser, new SourceForgeMirrorListDownloader(mirrorListUrl)) {}

        public SourceForgeMirrorProvider(SourceForgeMirrorParser parser, ISourceForgeMirrorListDownloader downloader)
        {
            listDownloader = downloader;
            this.parser = parser;
            Name = "SourceForge.net";
        }

        /// <summary>
        /// The name of the mirror provider
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets a list of mirrors for this provider
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileMirror> GetMirrors()
        {
            return GetMirrors(DownloadMirrorList());
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Gets a list of mirrors for this provider from the passed HTML
        /// </summary>
        /// <param name="html">The HTML containing the mirror list</param>
        /// <returns></returns>
        public IEnumerable<FileMirror> GetMirrors(string html)
        {
            return parser.ParseMirrors(html);
        }

        /// <summary>
        /// Downloads the list of mirrors
        /// </summary>
        /// <returns></returns>
        public string DownloadMirrorList()
        {
            return listDownloader.Download();
        }

        public string GetUrlForMirror(FileMirror mirror)
        {
            return string.Format(CultureInfo.CurrentCulture, "https://downloads.sourceforge.net/sourceforge/emulepawcio/ipfilter.zip?use_mirror={0}", mirror.Id);
        }
    }
}