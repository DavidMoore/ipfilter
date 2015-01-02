namespace IPFilter
{
    using System.Threading.Tasks;
    using Models;

    public interface ICacheProvider
    {
        FilterDownloadResult Get(FilterDownloadResult filter);
        Task SetAsync(FilterDownloadResult filter);
    }
}