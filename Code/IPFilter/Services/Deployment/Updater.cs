using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;

namespace IPFilter.Services
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Models;

    class Updater
    {        
        public async Task<UpdateInfo> CheckForUpdateAsync()
        {
            const string latestReleases = "https://api.github.com/repos/DavidMoore/IPFilter/releases";

            using(var handler = new WebRequestHandler())
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DavidMooreIPFilter", EntryPoint.Version.ToString()));

                // Get the latest releases from GitHub
                using (var response = await client.GetAsync(latestReleases,HttpCompletionOption.ResponseHeadersRead))
                {
                    var content = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();

                    var serializer = new JavaScriptSerializer();
                    var results = serializer.Deserialize<List<GitHubRelease>>(content);

                    var latest = results.FirstOrDefault(x => !x.tag_name.Equals("lists", StringComparison.OrdinalIgnoreCase));
                    if (latest == null)
                    {
                        Trace.TraceWarning("Couldn't find a release from the list returned by GitHub");
                        return null;
                    }

                    var asset = latest.assets.FirstOrDefault(x => x.name.Equals("IPFilter.msi", StringComparison.OrdinalIgnoreCase));
                    if (asset == null)
                    {
                        Trace.TraceWarning("Couldn't find installer in the release assets for " + latest.name);
                        return null;
                    }

                    var info = new UpdateInfo
                    {
                        Version = latest.tag_name,
                        Uri = asset.browser_download_url.ToString()
                    };

                    return info;
                }
            }
        }

    }

    class GitHubRelease
    {
        public string name { get; set; }

        public string tag_name { get; set; }

        public ICollection<GitHubAsset> assets { get;set; }
    }

    class GitHubAsset
    {
        public long id { get; set; }

        public string name { get; set; }

        public Uri browser_download_url { get; set; }

        public string content_type { get; set; }

        public long size { get;set; }

        public DateTimeOffset created_at { get;set; }

        public DateTimeOffset updated_at { get;set; }
    }
}