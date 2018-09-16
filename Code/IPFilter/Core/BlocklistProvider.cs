using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IPFilter.Core
{
    [DataContract]
    public class BlocklistProvider
    {
        [DataMember(Name="id")]
        public string Id { get; set; }
        [DataMember(Name="name")]
        public string Name { get; set; }
        [DataMember(Name="description")]
        public string Description { get; set; }
        [DataMember(Name="baseUri")]
        public Uri BaseUri { get; set; }

        [DataMember(Name="uri")]
        public Uri Uri { get; set; }

        [DataMember(Name="lists")]
        public IList<Blocklist> Lists { get; set; }
    }
}