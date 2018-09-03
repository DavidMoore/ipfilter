using System.Threading.Tasks;

namespace IPFilter.Cli
{
    interface INode
    {
        Task Accept(INodeVisitor visitor);
    }
}