namespace IPFilter.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Properties;

    public class DestinationPathsProvider
    {
        public IEnumerable<string> GetDestinations(params string[] values)
        {
            if (values == null) return Enumerable.Empty<string>();

            return values.Select(Environment.ExpandEnvironmentVariables)
                         .Select(TrimSeparatorsAndWhitespace)
                         .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        string TrimSeparatorsAndWhitespace(string value)
        {
            // TODO: Handle unicode whitespace?
            char[] trimChars = {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, ' ', '\t'};

            return value.TrimStart().TrimEnd(trimChars);
        }

        public IEnumerable<PathSetting> GetDestinations()
        {
            try
            {
                // Try to combine our defaults with the custom ones
                if (Config.Default.outputs == null)
                {
                    return Enumerable.Empty<PathSetting>();
                }
                
                return Config.Default.outputs.Select(ParseCustomPath);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Had trouble getting the custom paths: " + ex);
                return Enumerable.Empty<PathSetting>();
            }
        }

        PathSetting ParseCustomPath(string arg)
        {
            var separatorIndex = arg.IndexOf(';');
            var name = "(Untitled)";
            string path;

            if (separatorIndex > -1)
            {
                name = arg.Substring(0, separatorIndex);
                path = arg.Substring(separatorIndex);
            }
            else
            {
                path = arg;
            }

            path = TrimSeparatorsAndWhitespace(path);

            return new PathSetting(name, path);
        }
    }
}