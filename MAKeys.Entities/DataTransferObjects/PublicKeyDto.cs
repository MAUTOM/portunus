using System;
using System.Collections.Generic;
using MAKeys.Shared;

namespace MAKeys.Entities.DataTransferObjects
{
    public sealed class PublicKeyDto
    {
        public string Fingerprint { get; set; } = null!;

        public ICollection<KeyIdentityDto> KeyIdentities { get; set; } = null!;

        public string ArmoredKey { get; set; } = null!;

        public PublicKeyAlgorithm Algorithm { get; set; }

        public PublicKeyLength Length { get; set; }

        public PublicKeyFlags Flags { get; set; }

        public DateTime SubmissionDate { get; set; }
        
        public DateTime CreationDate { get; set; }
        
        public DateTime? ExpirationDate { get; set; }
    }
}