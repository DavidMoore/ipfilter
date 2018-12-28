using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using IPFilter.Cli;
using IPFilter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IPFilter.Tests
{
    [TestClass]
    public class ZipEnumeratorTests
    {
        [TestMethod]
        public async Task GetFilters()
        {
            using (var temp = new TempFile())
            {
                using (var stream = temp.OpenWrite())
                {
                    using (var zip = StreamHelper.CreateZipArchive(stream, ZipArchiveMode.Create))
                    {
                        StreamHelper.CreateZipArchiveEntry(zip, "binary.txt", TestFilterData.TextWithMixedBinary);
                        StreamHelper.CreateZipArchiveEntry(zip, "text.txt", TestFilterData.TextWithBlankLines);
                        StreamHelper.CreateZipArchiveEntry(zip, "text2.txt", TestFilterData.TextMixedLineEndings);
                    }
                }

                var context = new Mock<FilterContext>();
                context.Setup(x => x.CancellationToken).Returns(CancellationToken.None);
                var visitor = new Mock<INodeVisitor>();
                visitor.Setup(x => x.Context).Returns(context.Object);
                visitor.Setup(x => x.Visit(It.IsAny<FileNode>())).Returns(Task.FromResult(1));

                var node = new ZipNode(temp.File);

                await node.Accept(visitor.Object);

                visitor.Verify(x => x.Visit(It.IsAny<FileNode>()), Times.Exactly(3));
            }
        }
    }
}