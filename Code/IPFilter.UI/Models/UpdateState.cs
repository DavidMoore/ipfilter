namespace IPFilter.UI
{
    public enum UpdateState
    {
        /// <summary>
        /// Ready to download
        /// </summary>
        Ready,

        /// <summary>
        /// The download is in progress
        /// </summary>
        Downloading,

        /// <summary>
        /// The download is done
        /// </summary>
        Done,

        /// <summary>
        /// Is download, but the user has clicked "Cancel" and the
        /// download should be stopped as soon as possible
        /// </summary>
        Cancelling,

        /// <summary>
        /// The download was cancelled
        /// </summary>
        Cancelled,

        Decompressing
    }
}