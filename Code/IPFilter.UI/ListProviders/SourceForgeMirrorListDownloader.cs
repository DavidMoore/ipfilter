namespace IPFilter.ListProviders
{
    using System;
    using System.IO;
    using System.Net;

    public class SourceForgeMirrorListDownloader : ISourceForgeMirrorListDownloader
    {
        public SourceForgeMirrorListDownloader(string listUrl) : this(new Uri(listUrl)) {}

        public SourceForgeMirrorListDownloader(Uri listUrl)
        {
            if( listUrl == null) throw new ArgumentNullException("listUrl");

            ListUrl = listUrl;
        }

        /// <summary>
        /// Downloads the list
        /// </summary>
        /// <returns></returns>
        public string Download()
        {
            var request = WebRequest.Create(ListUrl);
            using(var response = request.GetResponse())
            using(var stream = response.GetResponseStream())
            using(var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// URL that returns a list of mirrors
        /// </summary>
        public Uri ListUrl { get; set; }
    }
}