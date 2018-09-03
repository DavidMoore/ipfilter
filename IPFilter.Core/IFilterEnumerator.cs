using System.Threading.Tasks;

namespace IPFilter.Core
{
    /// <summary>
    /// Contract for a type that reads in a list of filters
    /// and writes them to a destination.
    /// </summary>
    public interface IFilterEnumerator
    {
        /// <summary>
        /// Writes a list of filters to the specified <see cref="writer"/>.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task GetFilters(IFilterWriter writer, FilterContext context);
    }
}