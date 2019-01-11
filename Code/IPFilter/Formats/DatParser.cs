using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using IPFilter.Models;

namespace IPFilter.Formats
{
    /// <summary>
    /// Support for parsing eMule Text Format (DAT)
    /// </summary>
    public class DatParser
    {
        static readonly StringBuilder sb = new StringBuilder(33);

        public static string ParseLine(string line)
        {
            if (line == null) return null;
            var value = line.Trim();

            // Ignore comment lines
            if (value[0] == '#' || value.StartsWith("//")) return null;

            // The first part is the IP address range, with optional parts delimited by comma
            var firstDelimiter = value.IndexOf(',');
            var range = value.Substring(0, firstDelimiter > 0 ? firstDelimiter : value.Length);

            // Check for the access level. 128 or higher means to ignore the line (not block).
            if (firstDelimiter > 0)
            {
                var accessDelimiter = value.IndexOf(',', firstDelimiter + 1);
                var accessText = accessDelimiter > -1 ? value.Substring(firstDelimiter + 1, accessDelimiter - firstDelimiter - 1) : value.Substring(firstDelimiter + 1);

                // Skip lines with access higher than 127
                if (int.TryParse(accessText, out var access) && access > 127) return null;

                if (accessDelimiter > -1)
                {
                    var descriptionDelimiter = value.IndexOf(',', accessDelimiter + 1);
                    var description = descriptionDelimiter > -1 ? value.Substring(accessDelimiter + 1, value.Length - descriptionDelimiter) : value.Substring(accessDelimiter + 1);
                }
            }

            var dash = range.IndexOf('-');
            if (dash == -1 || dash >= range.Length - 1)
            {
                Trace.TraceWarning("Malformed line: " + line);
                return null;
            }

            var fromText = range.Substring(0, dash).Trim();
            var toText = range.Substring(dash + 1).Trim();
            
            var from = IpAddress.Parse(fromText);
            if( from == 0) return null;

            var to = IpAddress.Parse(toText);
            if (to == 0) return null;
            
            // The from address must come before the to address (or be the same).
            if (from > to) return null;

            
            var fromBytes = IpAddress.GetBytes(from);
            var toBytes = IpAddress.GetBytes(to);

            sb.Clear();

            sb.Append(fromBytes[0]).Append(".")
                .Append(fromBytes[1]).Append(".")
                .Append(fromBytes[2]).Append(".")
                .Append(fromBytes[3]).Append(" - ")
                .Append(toBytes[0]).Append(".")
                .Append(toBytes[1]).Append(".")
                .Append(toBytes[2]).Append(".")
                .Append(toBytes[3]);

            return sb.ToString();
        }

        public static FilterEntry ParseEntry(string line)
        {
            if (line == null) return null;
            var value = line.Trim();

            // Ignore comment lines
            if (value[0] == '#' || value.StartsWith("//")) return null;

            byte level = 0;
            string description = null;

            // The first part is the IP address range, with optional parts delimited by comma
            var firstDelimiter = value.IndexOf(',');
            var range = value.Substring(0, firstDelimiter > 0 ? firstDelimiter : value.Length);

            // Check for the access level. 128 or higher means to ignore the line (not block).
            if (firstDelimiter > 0)
            {
                var accessDelimiter = value.IndexOf(',', firstDelimiter + 1);
                var accessText = accessDelimiter > -1 ? value.Substring(firstDelimiter + 1, accessDelimiter - firstDelimiter - 1) : value.Substring(firstDelimiter + 1);

                // Skip lines with access higher than 127
                if (byte.TryParse(accessText, out var access))
                {
                    level = access;
                }

                if (accessDelimiter > -1)
                {
                    var descriptionDelimiter = value.IndexOf(',', accessDelimiter + 1);
                    description = (descriptionDelimiter > -1 ? value.Substring(accessDelimiter + 1, value.Length - descriptionDelimiter) : value.Substring(accessDelimiter + 1)).Trim();
                }
            }

            var dash = range.IndexOf('-');
            if (dash == -1 || dash >= range.Length - 1)
            {
                Trace.TraceWarning("Malformed line: " + line);
                return null;
            }

            var fromText = range.Substring(0, dash).Trim();
            var toText = range.Substring(dash + 1).Trim();
            
            var from = IpAddress.Parse(fromText);
            if (from == 0) return null;

            var to = IpAddress.Parse(toText);
            if (to == 0) return null;
            
            // The from address must come before the to address (or be the same).
            if (from > to) return null;

            return new FilterEntry(from, to)
            {
                Description = description,
                Level = level
            };
        }
    }
}