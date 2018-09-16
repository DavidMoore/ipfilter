using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IPFilter.Cli
{
    class UriNode : INode
    {
        static readonly MD5 hasher = MD5.Create();
        readonly Uri uri;
        string uriHash;
        readonly FileFetcher fileFetcher;

        public UriNode(Uri uri)
        {
            this.uri = uri;
            this.uriHash = hasher.ComputeHash(uri);
            fileFetcher = new CachingFileFetcher();
        }

        public async Task Accept(INodeVisitor visitor)
        {
            var node = await fileFetcher.Get(uri, visitor.Context);
            if (node == null) return;
            await visitor.Visit(node);
        }
    }
}