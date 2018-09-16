using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using IPFilter.Core;

namespace IPFilter.Cli
{
    class JsonNode : INode
    {
        readonly FileInfo file;
        static readonly DataContractJsonSerializer serializer;

        static JsonNode()
        {
            var settings = new DataContractJsonSerializerSettings
            {
                KnownTypes = new []{typeof(BlocklistBundle), typeof(BlocklistProvider), typeof(Blocklist)}
            };

            serializer = new DataContractJsonSerializer(typeof(BlocklistBundle), settings);
        }

        public JsonNode(FileInfo file)
        {
            this.file = new FileInfo(file.FullName);
        }

        public async Task Accept(INodeVisitor visitor)
        {
            BlocklistBundle bundle = null;

            var json = await file.ReadAllText();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                try
                {
                    bundle = (BlocklistBundle) serializer.ReadObject(stream);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Couldn't read list json: " + ex);
                }
            }

            if (bundle == null)
            {
                Trace.TraceWarning($"Couldn't read JSON file {file.FullName}");
                return;
            }

            foreach (var provider in bundle.Lists)
            {
                if (visitor.Context.CancellationToken.IsCancellationRequested) return;

                if (provider?.Lists == null) continue;

                foreach (var list in provider.Lists)
                {
                    var uri = ResolveUri(list, provider);
                    if (uri == null) continue;
                    if (visitor.Context.CancellationToken.IsCancellationRequested) return;
                    
                    // Process this list
                    await visitor.Visit(new UriNode(uri));
                }
            }
        }


        Uri ResolveUri(Blocklist list, BlocklistProvider provider)
        {
            var uri = list.Uri;

            if (uri != null && uri.IsAbsoluteUri) return uri;

            if (provider.BaseUri == null)
            {
                Trace.TraceWarning("Skipping {0} as it doesn't have a full URI, and parent {1} has no base URI we can use.", list.Name, provider.Name);
                return null;
            }

            var resolved = string.Format(provider.BaseUri.ToString(), uri?.ToString() ?? list.Id);
            uri = new Uri(provider.BaseUri, new Uri(resolved));

            return uri;
        }
    }
}