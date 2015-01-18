namespace IPFilter.Services
{
    public class PathSetting
    {
        public PathSetting(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; set; }
        public string Path { get; set; }
    }
}