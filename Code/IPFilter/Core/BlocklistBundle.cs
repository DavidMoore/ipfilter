using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IPFilter.Core
{
    [DataContract]
    public class BlocklistBundle
    {
        [DataMember(Name = "name", IsRequired = false)]
        public string Name { get; set; }

        [DataMember(Name="description", IsRequired = false)]
        public string Description { get; set; }

        [DataMember(Name="lists")]
        public IList<BlocklistProvider> Lists { get; set; }
    }
}