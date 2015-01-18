namespace IPFilter.ListProviders
{
    using System.Collections.Generic;
    using Models;

    /// <summary>
    /// Common interface for classes that can parse SourceForge
    /// mirror list HTML into a collection of <see cref="FileMirror"/>
    /// objects
    /// </summary>
    public interface ISourceForgeMirrorParser
    {
        /// <summary>
        /// Parses some passed HTML received from SourceForge for
        /// available file mirrors
        /// </summary>
        /// <param name="mirrorsHtml"></param>
        /// <returns></returns>
        IEnumerable<FileMirror> ParseMirrors(string mirrorsHtml);
    }
}