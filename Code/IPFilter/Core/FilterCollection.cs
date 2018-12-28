using System;
using System.Collections.Generic;
using IPFilter.Models;

namespace IPFilter.Core
{
#pragma warning disable 618
    public class FilterCollection : SortedSet<FilterEntry>
    {
        static readonly IComparer<FilterEntry> comparer = new MutableFilterEntryComparer();

        public static IList<FilterEntry> Sort(IEnumerable<FilterEntry> source)
        {
            var list = new List<FilterEntry>(source);
            list.Sort(FilterEntry.Comparer);
            return list;
        }

        public static IList<FilterEntry> Merge(IEnumerable<FilterEntry> source)
        {
            var list = new List<FilterEntry>(source);
            list.Sort(FilterEntry.Comparer);

            for (var i = 0; i < list.Count; i++)
            {
                var filter = list[i];

                // Keep peeking at the next entries to see if they fit in range, in which case we will merge them.
                while ( i + 1 < list.Count )
                {
                    var next = list[i + 1];

                    if( comparer.Compare(filter, next) == 0)
                    {
                        list.RemoveAt(i + 1);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            
            return list;
        }

        class MutableFilterEntryComparer : IComparer<FilterEntry>
        {
            public int Compare(FilterEntry x, FilterEntry y)
            {
                if (x == null || y == null)
                {
                    if (x == null && y == null) return 0;
                    return x == null ? -1 : 1;
                }

                if ( FilterEntry.AddressComparer.Compare(x.From, y.From) < 1 && FilterEntry.AddressComparer.Compare(y.From, x.To) <= 1)
                {
                    // The two ranges overlap, so merge them.
                    x.From.Address = Math.Min(x.From.Address, y.From.Address);
                    x.To.Address = Math.Max(x.To.Address, y.To.Address);
                    return 0;
                }

                return FilterEntry.Comparer.Compare(x, y);
            }
        }
    }
#pragma warning restore 618
}