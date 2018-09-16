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

        async Task<DataFormat> DetectFormat()
        {
            using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var buffer = new byte[4];
                var bytesRead = await stream.ReadAsync(buffer, 0, 4);
                if (bytesRead == 4)
                {
                    // Look for the GZip header bytes
                    if (buffer[0] == 31 && buffer[1] == 139) return DataFormat.GZip;

                    // Look for the ZIP header bytes.
                    var zipHeaderNumber = BitConverter.ToInt32(buffer, 0);
                    if (zipHeaderNumber == 0x4034b50) return DataFormat.Zip;
                }

                stream.Seek(0, SeekOrigin.Begin);

                // Read the first line
                using (var reader = new StreamReader(stream))
                {
                    var lineBuffer = new char[1000];
                    var charsRead = await reader.ReadBlockAsync(lineBuffer, 0, lineBuffer.Length);

                    var sb = new StringBuilder(lineBuffer.Length);

                    for (var i = 0; i < charsRead; i++)
                    {
                        var character = lineBuffer[i];

                        // We only want up until the first line
                        if (character == '\n' || character == '\r') break;

                        // If we see non-text characters, it's not a text file
                        if (char.IsControl(character)) return DataFormat.Binary;

                        sb.Append(character);
                    }

                    // This looks like a text file, but is it maybe JSON?
                    if (sb.ToString().TrimStart().StartsWith("{")) return DataFormat.Json;

                    return DataFormat.Text;
                }
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