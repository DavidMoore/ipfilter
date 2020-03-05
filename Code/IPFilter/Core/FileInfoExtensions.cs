using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IPFilter.Cli
{
    public static class FileInfoExtensions
    {
        public static void SafeDelete(this FileInfo file)
        {
            if (file == null) return;

            try
            {
                file.Refresh();
                if (!file.Exists) return;
                if (file.IsReadOnly) file.IsReadOnly = false;
                file.Delete();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Couldn't delete file {file.FullName}: {ex}");
            }
        }

        public static async Task<string> ReadAllText(this FileInfo file)
        {
            using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            using(var reader = new StreamReader(stream, Encoding.UTF8, true))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// Writes the passed string to the file using UTF-8 encoding, creating the file if it doesn't exist,
        /// and overwriting if it does.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task WriteAllText(this FileInfo file, string value)
        {
            using (var stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
            using(var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                await writer.WriteAsync(value);
            }
        }
    }
}