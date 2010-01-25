namespace IPFilter
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

        public override string ToString()
        {
            return Name;
        }
    }
}