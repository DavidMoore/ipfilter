using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.AccessControl;
using System.Threading.Tasks;
using IPFilter.Core;

namespace IPFilter.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var options = Options.Parse(args);

            var context = new FilterContext();

            // Configure outputs
            if (options.Outputs.Count > 0)
            {
                context.Filter = new TextFilterWriter(options.Outputs.First());
            }
            
            // Resolve the input URIs to nodes to visit
            var nodes = new List<UriNode>();

            foreach (var input in options.Inputs)
            {
                var uri = context.UriResolver.Resolve(input);
                if (uri == null) continue;
                nodes.Add(new UriNode(uri));
            }

            using (INodeVisitor visitor = new NodeVisitor(context))
            {
                foreach (var node in nodes)
                {
                    await visitor.Visit(node);
                }
            }
        }
    }
}
