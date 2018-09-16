namespace IPFilter.ListProviders
{
    public class DefaultList : IMirrorProvider
    {
        public string Name => "Default";

        public string GetUrlForMirror()
        {
            return "https://github.com/DavidMoore/ipfilter/releases/download/lists/ipfilter.dat.gz";
        }
    }
}