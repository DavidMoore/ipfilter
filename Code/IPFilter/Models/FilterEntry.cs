using System;
using System.Collections.Generic;
using System.Net;
using IPFilter.Core;
using IPFilter.Formats;
using IPFilter.UI.Annotations;

namespace IPFilter.Models
{
    /// <summary>
    /// Information about a blocked range of IP addresses.
    /// </summary>
    public class FilterEntry : IEquatable<FilterEntry>
    {
        public static IComparer<FilterEntry> Comparer { get; } = new FilterEntryComparer();

        public static IPAddressComparer AddressComparer { get; } = new IPAddressComparer();

        public FilterEntry([NotNull] string from, [NotNull] string to) : this(DatParser.ParseAddress(from), DatParser.ParseAddress(to)) { }

        public FilterEntry([NotNull] IPAddress from, [NotNull] IPAddress to)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
        }

        /// <summary>
        /// The filter level. Anything <c>128</c> or higher isn't blocked.
        /// </summary>
        public byte Level { get; set; }

        /// <summary>
        /// The optional description of the list entry (for example, why it was added to a block list)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// For debug purposes, the list URL or description that this entry originated from
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The bottom range of the blocked IP address(es). Inclusive.
        /// </summary>
        public IPAddress From { get; set; }

        /// <summary>
        /// The upper range of the blocked IP address(es). Inclusive.
        /// </summary>
        public IPAddress To { get; set; }

        public override string ToString()
        {
            return $"[{From}-{To}]";
        }

        sealed class FilterEntryComparer : IComparer<FilterEntry>
        {
            public int Compare(FilterEntry x, FilterEntry y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                var from = AddressComparer.Compare(x.From, y.From);
                return from != 0 ? from : AddressComparer.Compare(x.To, y.To);
            }
        }

        public bool Equals(FilterEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return From.Equals(other.From) && To.Equals(other.To);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FilterEntry) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ( AddressComparer.GetHashCode(From) * 397) ^ AddressComparer.GetHashCode(To);
            }
        }

        public static bool operator ==(FilterEntry left, FilterEntry right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FilterEntry left, FilterEntry right)
        {
            return !Equals(left, right);
        }

        public static bool operator >(FilterEntry left, FilterEntry right)
        {
            return Comparer.Compare(left, right) > 0;
        }

        public static bool operator <(FilterEntry left, FilterEntry right)
        {
            return Comparer.Compare(left, right) < 0;
        }
    }
}
