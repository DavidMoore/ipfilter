namespace IPFilter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

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
    }
}