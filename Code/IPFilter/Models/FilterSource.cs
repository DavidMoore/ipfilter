using System;

namespace IPFilter.Models
{
    public class FilterSource
    {
        public bool IsSelected { get; set; }

        public string Provider { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }


        public DateTimeOffset? LastModified { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        private bool Equals(FilterSource other)
        {
            return string.Equals(Provider, other.Provider, StringComparison.OrdinalIgnoreCase) && string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FilterSource) obj);
        }

        public override int GetHashCode()
        {
            if (Provider == null || Id == null) return 0;
            return (StringComparer.OrdinalIgnoreCase.GetHashCode(Provider) * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Id);
        }
    }
}