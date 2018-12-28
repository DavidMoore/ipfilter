using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IPFilter.Core;

namespace IPFilter.Cli
{
    class FileNode : INode
    {
        readonly FileInfo file;

        public FileNode(FileInfo file)
        {
            this.file = new FileInfo(file.FullName);
        }

        public string FullName => file.FullName;

        internal Task<DataFormat> DetectFormat()
        {
            using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return FormatDetector.DetectFormat(stream);
            }
        }

        public async Task Accept(INodeVisitor visitor)
        {
            // Find the file type
            var fileType = await DetectFormat();
            
            switch (fileType)
            {
                case DataFormat.GZip:
                    await visitor.Visit(new GZipNode(file));
                    break;

                case DataFormat.Zip:
                    await visitor.Visit(new ZipNode(file));
                    break;

                case DataFormat.Json:
                    await visitor.Visit(new JsonNode(file));
                    break;

                case DataFormat.Text:
                    await visitor.Visit(new TextNode(file));
                    break;

                case DataFormat.Binary:
                default:
                    break;
            }
        }
    }
}