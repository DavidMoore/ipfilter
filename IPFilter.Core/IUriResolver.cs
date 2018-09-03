using System;

namespace IPFilter.Core
{
    public interface IUriResolver
    {
        Uri Resolve(string url);
    }
}