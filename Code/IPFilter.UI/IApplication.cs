namespace IPFilter
{
    using System.Threading.Tasks;

    interface IApplication
    {
        Task<ApplicationDetectionResult> DetectAsync();

        Task<FilterUpdateResult> UpdateFilterAsync(IProgress<int> progress);
    }
}