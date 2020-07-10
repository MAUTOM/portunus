using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MAKeys.Shared;

namespace MAKeys.Entities.Models
{
    [Table("PublicKeys")]
    public sealed class PublicKey
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Key fingerprint must be specified.")]
        [StringLength(40, MinimumLength = 40, ErrorMessage = "Key fingerprint must be 40 characters long.")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Fingerprint { get; set; } = null!;
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "No public key data!")]
        [DataType(DataType.Text)]
        public string ArmoredKey { get; set; } = null!;
        
        public DateTime SubmissionDate { get; set; }
        
        [Required]
        public DateTime CreationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        
        public PublicKeyFlags Flags { get; set; }
        public PublicKeyAlgorithm Algorithm { get; set; }
        public PublicKeyLength Length { get; set; }

        public ICollection<KeyIdentity> KeyIdentities { get; set; } = null!;
    }
}