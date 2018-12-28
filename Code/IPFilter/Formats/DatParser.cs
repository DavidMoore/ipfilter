using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using IPFilter.Models;

namespace IPFilter.Formats
{
    /// <summary>
    /// Support for parsing eMule Text Format (DAT)
    /// </summary>
    public class DatParser
    {
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
            
            IPAddress from = ParseAddress(fromText);
            if( from == null) return null;

            IPAddress to = ParseAddress(toText);
            if (to == null) return null;
            
            // The ranges must be the same format
            if (from.AddressFamily != to.AddressFamily) return null;

            // The from address must come before the to address (or be the same).
            if (from.AddressFamily > to.AddressFamily) return null;
            
            return from + " - " + to;
        }

        internal static IPAddress ParseAddress(string address)
        {
            if (string.IsNullOrEmpty(address)) return null;

            if (address.IndexOf(':') == -1)
            {
                // Treat as an IPv4 address
                var numbers = address.Split(new[] { '.' }, 4);
                if (numbers.Length < 4) return null;

                var bytes = new byte[4];

                for (var i = 0; i < numbers.Length; i++)
                {
                    if (!int.TryParse(numbers[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)) return null;
                    if (number < 0 || number > 255) return null;
                    bytes[i] = (byte) number;
                }

                return new IPAddress(bytes);
            }

            try
            {
                return IPAddress.Parse(address);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static FilterEntry ParseEntry(string line)
        {
            if (line == null) return null;
            var value = line.Trim();

            // Ignore comment lines
            if (value[0] == '#' || value.StartsWith("//")) return null;

            byte level = 0;
            string description = null;
            IPAddress from = null;
            IPAddress to = null;

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

            from = ParseAddress(fromText);
            if (from == null) return null;

            to = ParseAddress(toText);
            if (to == null) return null;

            // The ranges must be the same format
            if (from.AddressFamily != to.AddressFamily) return null;

            // The from address must come before the to address (or be the same).
            if (from.AddressFamily > to.AddressFamily) return null;

            return new FilterEntry(from, to)
            {
                Description = description,
                Level = level
            };
        }
    }
}