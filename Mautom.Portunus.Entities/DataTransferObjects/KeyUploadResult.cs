using System;
using System.Collections.Generic;
using Mautom.Portunus.Shared.Pgp;
using Newtonsoft.Json;

namespace Mautom.Portunus.Entities.DataTransferObjects
{

    public class KeyUploadResult
    {
        public KeyUploadResult(string fingerprint, Guid token, IDictionary<string, IdentityStatus> identityStatuses)
        {
            IdentityStatuses = identityStatuses;
            Fingerprint = fingerprint;
            Token = token;
        }

        [JsonProperty(PropertyName = "key_fpr")]
        public string Fingerprint { get; }
        [JsonProperty(PropertyName = "status")]
        public IDictionary<string, IdentityStatus> IdentityStatuses;
        [JsonProperty(PropertyName = "token")]
        public Guid Token { get; }
    }
}