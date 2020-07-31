using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mautom.Portunus.Entities.DataTransferObjects
{
    public class RequestVerifyParameters
    {
        public RequestVerifyParameters(Guid token, IList<string> addresses, IList<string> locales)
        {
            Token = token;
            Addresses = addresses;
            Locales = locales;
        }

        [JsonProperty(PropertyName = "token")]
        public Guid Token { get; set; }
        
        [JsonProperty(PropertyName = "addresses")]
        public IList<string> Addresses { get; set; }
        
        [JsonProperty(PropertyName = "locale")]
        public IList<string> Locales { get; set; }
    }
}