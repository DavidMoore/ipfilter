using System.Runtime.Serialization;

namespace IPFilter.Models
{
    [DataContract]
    public class UpdateInfo
    {
        [DataMember(Name="version")]
        public string Version { get; set; }

        [DataMember(Name="uri")]
        public string Uri { get; set; }
    }
}