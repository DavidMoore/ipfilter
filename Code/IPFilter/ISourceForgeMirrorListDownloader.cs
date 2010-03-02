using System;

namespace IPFilter
{
    public interface ISourceForgeMirrorListDownloader {
        /// <summary>
        /// Downloads the list
        /// </summary>
        /// <returns></returns>
        string Download();

        /// <summary>
        /// URL that returns a list of mirrors
        /// </summary>
        Uri ListUrl { get; set; }
    }
}