using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using IPFilter.UI.Annotations;

namespace IPFilter.Native
{
    static class PathHelper
    {
        public static DirectoryInfo GetDirectoryInfo(string path)
        {
            return GetDirectoryInfo(path, CultureInfo.CurrentCulture);
        }

        public static DirectoryInfo GetDirectoryInfo([NotNull] string path, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            try
            {
                // On some cultures, the backslash character (0x5C / 92) is displayed differently, so we will use the culture's codepage
                // to read in the directory, and then convert it back, which should fix the backslashes.

                // For example, in Japanese codepage 932, the backslash character is displayed as the Yen symbol e.g C:¥Program Files¥qBittorrent
                // If we read in the path bytes using the Japanese codepage of 932, then convert those bytes to a string using the same codepage,
                // the yen character will actually get corrected to the backslash.
            
                // Get the culture's codepage
                var encoding = Encoding.GetEncoding(cultureInfo.TextInfo.ANSICodePage);

                var normalizedPath = encoding.GetString(encoding.GetBytes(path));
                
                return new DirectoryInfo(normalizedPath);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning($"Couldn't normalize the path '{path}'. Culture: {cultureInfo}, CodePage: {cultureInfo.TextInfo.ANSICodePage}");
                Trace.TraceWarning("Error: " + ex);
                Trace.TraceInformation($"Falling back to un-normalized path of '{path}'");

                return new DirectoryInfo(path);
            }
        }
    }
}