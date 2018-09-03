using System;
using System.Threading.Tasks;
using IPFilter.Core;

namespace IPFilter.Cli
{
    class NodeVisitor : INodeVisitor
    {
        public NodeVisitor() : this(new FilterContext()) {}

        public NodeVisitor(FilterContext context)
        {
            Context = context;
        }

        public FilterContext Context { get; }

        public Task Visit<T>(T node) where T : INode
        {
            Console.WriteLine("Visiting " + node);
            return node.Accept(this);
        }

//        public async Task Visit(UriNode node)
//        {
//            await node.Accept(this);
//        }
//
//        public Task Visit(FileNode node)
//        {
//            throw new NotImplementedException();
//        }
        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}