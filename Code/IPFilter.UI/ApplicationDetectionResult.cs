namespace IPFilter
{
    using System.IO;

    class ApplicationDetectionResult
    {
        public bool IsPresent { get; set; }

        public string Description { get; set; }

        public DirectoryInfo InstallLocation { get; set; }

        public string Version { get; set; }

        public static ApplicationDetectionResult NotFound()
        {
            return new ApplicationDetectionResult() {IsPresent = false};
        }
    }
}