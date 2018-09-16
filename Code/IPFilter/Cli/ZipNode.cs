using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace IPFilter.Cli
{
    class ZipNode : INode
    {
        readonly FileInfo file;

        public ZipNode(FileInfo file)
        {
            this.file = new FileInfo(file.FullName);
        }

        public async Task Accept(INodeVisitor visitor)
        {
            using(var zipSource = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var zipFile = new ZipArchive(zipSource, ZipArchiveMode.Read, true))
            {
                foreach (var entry in zipFile.Entries)
                {
                    using (var entryStream = entry.Open())
                    using (var tempFile = new TempFile())
                    {
                        using (var tempStream = tempFile.OpenWrite())
                        {
                            await entryStream.CopyToAsync(tempStream, visitor.Context.CancellationToken);
                        }

                        await visitor.Visit(new FileNode(tempFile.File));
                    }
                }
            }
        }
    }
}