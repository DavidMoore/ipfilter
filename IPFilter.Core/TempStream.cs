using System.IO;
using IPFilter.Cli;

namespace IPFilter.Core
{
    public class TempStream : FileStream
    {
        readonly FileInfo file;

        public FileInfo File => file;

        public TempStream(FileInfo file) : base(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Read)
        {
            this.file = new FileInfo(file.FullName);
        }

        ~TempStream()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            file.SafeDelete();
        }
    }
}