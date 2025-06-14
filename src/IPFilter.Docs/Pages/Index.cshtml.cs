using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Octokit;

namespace IPFilter.Docs.Pages;
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public Release LatestClient { get; private set; }

    public ReleaseAsset LatestInstaller { get; private set; }

    public ReleaseAsset LatestExe { get; private set; }

    public Release Lists { get; private set; }

    public async Task OnGetAsync()
    {
        ProductHeaderValue header = new ProductHeaderValue("IPFilter");
        var client = new GitHubClient(header);
                
        LatestClient = await client.Repository.Release.GetLatest("DavidMoore", "IPFilter");
        LatestInstaller = LatestClient.Assets.Single(x => x.Name.Equals("IPFilter.msi", StringComparison.Ordinal));
        LatestExe = LatestClient.Assets.Single(x => x.Name.Equals("IPFilter.exe", StringComparison.Ordinal));
        Lists = await client.Repository.Release.Get("DavidMoore", "IPFilter", "lists");
    }
}
