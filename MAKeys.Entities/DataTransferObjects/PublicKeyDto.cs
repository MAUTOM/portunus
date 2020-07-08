using System;

namespace MAKeys.Entities.DataTransferObjects
{
    public sealed class PublicKeyDto
    {
        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Armor { get; set; } = null!;
        
        public string Fingerprint { get; set; } = null!;
        
        public DateTime SubmissionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}