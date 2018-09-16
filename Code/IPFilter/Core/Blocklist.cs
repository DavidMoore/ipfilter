using System;
using System.Runtime.Serialization;

namespace IPFilter.Core
{
    [DataContract]
    public class Blocklist
    {
        [DataMember(Name="id")]
        public string Id { get; set; }

        [DataMember(Name="name")]
        public string Name { get; set; }

        [DataMember(Name="description")]
        public string Description { get; set; }

        [DataMember(Name="uri")]
        public Uri Uri { get; set; }
    }
}