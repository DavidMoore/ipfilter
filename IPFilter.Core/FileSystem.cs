using System.IO;

namespace IPFilter.Core
{
    public class FileSystem : IFileSystem
    {
        public TempStream GetTempStream()
        {
            var file = new FileInfo(Path.GetTempFileName());
            return new TempStream(file);
        }
    }
}