namespace IPFilter.Core
{
    public interface IFileSystem
    {
        TempStream GetTempStream();
    }
}