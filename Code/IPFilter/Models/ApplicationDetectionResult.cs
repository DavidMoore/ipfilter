using IPFilter.Native;

namespace IPFilter.Models
{
    using System.IO;
    using Apps;

    public class ApplicationDetectionResult
    {
        public bool IsPresent { get; set; }

        public string Description { get; set; }

        public DirectoryInfo InstallLocation { get; set; }

        public string Version { get; set; }

        public IApplication Application { get; set; }

        public static ApplicationDetectionResult NotFound()
        {
            return new ApplicationDetectionResult();
        }

        public static ApplicationDetectionResult Create(IApplication application, string description, string installLocation)
        {
            var directory = PathHelper.GetDirectoryInfo(installLocation);
            
            return new ApplicationDetectionResult
            {
                Application = application, 
                Description = description, 
                InstallLocation = directory, 
                IsPresent = true
            };
        }
    }
}