using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IPFilter.Cli
{
    class TextNode : INode
    {
        readonly FileInfo file;

        public TextNode(FileInfo file)
        {
            this.file = new FileInfo(file.FullName);
        }

        public async Task Accept(INodeVisitor visitor)
        {
            using(var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            using(var reader = new StreamReader(stream, Encoding.UTF8))
            {
                if (visitor.Context.CancellationToken.IsCancellationRequested) return;

                var line = await reader.ReadLineAsync();
                while (line != null)
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length > 0)
                    {
                        // If we find any binary characters, skip this entire file.
//                        foreach (var character in trimmed)
//                        {
//                            if (char.IsControl(character)) return;
//                        }

                        await visitor.Context.Filter.WriteLineAsync(line);
                    }
                
                    if (visitor.Context.CancellationToken.IsCancellationRequested) return;
                    line = await reader.ReadLineAsync();
                }
            }
        }
    }
}