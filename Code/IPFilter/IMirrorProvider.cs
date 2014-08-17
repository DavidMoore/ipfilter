using System.Collections.Generic;

namespace IPFilter
{
    /// <summary>
    /// Contract for a source that provides mirrors of the file
    /// </summary>
    public interface IMirrorProvider
    {
        /// <summary>
        /// Gets a list of mirrors for this provider
        /// </summary>
        /// <returns></returns>
        IEnumerable<FileMirror> GetMirrors();

        /// <summary>
        /// The name of the mirror provider
        /// </summary>
        string Name { get; }

        string GetUrlForMirror(FileMirror mirror);
    }
}