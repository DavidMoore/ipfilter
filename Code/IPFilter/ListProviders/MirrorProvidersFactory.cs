using System.Collections.Generic;

namespace IPFilter.ListProviders
{
    static class MirrorProvidersFactory
    {
        private static readonly IList<IMirrorProvider> list = new List<IMirrorProvider> {new DefaultList()};

        public static IList<IMirrorProvider> Get()
        {
            return list;
        }
    }
}