using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Mautom.Portunus.Shared.Pgp
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum IdentityStatus
    {
        [StringValue("unpublished")]
        Unpublished,
        [StringValue("published")]
        Published,
        [StringValue("revoked")]
        Revoked,
        [StringValue("pending")]
        Pending
    }
}