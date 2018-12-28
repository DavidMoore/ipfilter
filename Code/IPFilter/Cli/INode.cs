using System.Threading.Tasks;

namespace IPFilter.Cli
{
    public interface INode
    {
        Task Accept(INodeVisitor visitor);
    }
}