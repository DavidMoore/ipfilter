namespace IPFilter.Services
{
    using System.Threading.Tasks;
    using Models;

    public interface ICacheProvider
    {
        Task<FilterDownloadResult> GetAsync(FilterDownloadResult filter);
        Task SetAsync(FilterDownloadResult filter);
    }
}