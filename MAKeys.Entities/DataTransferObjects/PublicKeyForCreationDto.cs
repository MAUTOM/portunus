using System;
using System.ComponentModel.DataAnnotations;
using MAKeys.Shared;

namespace MAKeys.Entities.DataTransferObjects
{
    public sealed class PublicKeyForCreationDto
    {
        [Required(ErrorMessage = "Fingerprint required!")]
        public string Fingerprint { get; set; } = null!;
        
        [Required(ErrorMessage = "Name for public key is required!")]
        [StringLength(50, ErrorMessage = "Name must not exceed 50 characters.")]
        public string Name { get; set; } = null!;
        
        [Required(ErrorMessage = "E-mail for public key is required!")]
        public string Email { get; set; } = null!;
        
        [Required(ErrorMessage = "No public key data")]
        public string ArmoredKey { get; set; } = null!;
        
        public PublicKeyAlgorithm Algorithm { get; set; }

        public PublicKeyLength Length { get; set; }

        public PublicKeyFlags Flags { get; set; }
        
        
        [Required(ErrorMessage = "Public key creation date is required.")]
        public DateTime CreationDate { get; set; }
        
        public DateTime SubmissionDate { get; set; }
        
        public DateTime? ExpirationDate { get; set; }
    }
}