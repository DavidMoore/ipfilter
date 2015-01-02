namespace IPFilter.UI.Apps
{
    class UTorrentApplication : BitTorrentApplication
    {
        protected override string DefaultDisplayName { get { return "µTorrent"; } }
        protected override string FolderName { get { return "uTorrent"; } }
    }
}