using System;
using System.Threading.Tasks;
using IPFilter.Core;

namespace IPFilter.Cli
{
    public interface INodeVisitor : IDisposable
    {
        FilterContext Context { get; }

        Task Visit<T>(T node) where T : INode;
    }
}