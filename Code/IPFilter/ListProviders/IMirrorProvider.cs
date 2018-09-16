namespace IPFilter.ListProviders
{
    /// <summary>
    /// Contract for a source that provides mirrors of the file
    /// </summary>
    public interface IMirrorProvider
    {
        /// <summary>
        /// The name of the mirror provider
        /// </summary>
        string Name { get; }

        string GetUrlForMirror();
    }
}