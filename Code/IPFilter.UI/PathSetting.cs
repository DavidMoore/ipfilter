namespace IPFilter.UI
{
    public class PathSetting
    {
        public PathSetting(string name, string path, bool isDefault)
        {
            Name = name;
            Path = path;
            IsDefault = isDefault;
        }

        public bool IsDefault { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}