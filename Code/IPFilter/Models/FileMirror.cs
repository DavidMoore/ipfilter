namespace IPFilter.Models
{
    public class FileMirror
    {
        public FileMirror(string id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Descriptive name of the mirror e.g. Transact (Canberra, Australia)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The id of the mirror e.g. transact
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the mirror provider
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Is the list selected
        /// </summary>
        public bool IsSelected { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}