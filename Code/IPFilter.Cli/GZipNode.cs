using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace IPFilter.Cli
{
    /// <summary>
    /// Decompresses a GZipped file and processes the contents.
    /// </summary>
    class GZipNode : INode
    {
        readonly FileInfo file;

        public GZipNode(FileInfo file)
        {
            this.file = new FileInfo(file.FullName);
        }

        public async Task Accept(INodeVisitor visitor)
        {
            // Extract the file to a temporary file, and then parse that.
            using (var tempFile = new TempFile())
            {
                // Extract to the temporary file
                using (var tempStream = tempFile.OpenWrite())
                using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var gzipFile = new GZipStream(stream, CompressionMode.Decompress, false))
                {
                    await gzipFile.CopyToAsync(tempStream);
                }

                // Process the extracted file
                await visitor.Visit(new FileNode(tempFile.File));
            }
        }
    }
}