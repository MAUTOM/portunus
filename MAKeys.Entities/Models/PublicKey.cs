using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAKeys.Entities.Models
{
    public class PublicKey
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Key fingerprint must be specified.")]
        [StringLength(40, MinimumLength = 40, ErrorMessage = "Key fingerprint must be 40 characters long.")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Fingerprint { get; set; } = null!;
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name for public key is required!")]
        [StringLength(50, ErrorMessage = "Name must not exceed 50 characters.")]
        public string Name { get; set; } = null!;
        
        [Required(ErrorMessage = "E-mail address is required for public key!")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "E-mail address")]
        [EmailAddress(ErrorMessage = "Invalid e-mail address")]
        public string Email { get; set; } = null!;
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "No public key data!")]
        [DataType(DataType.Text)]
        public string ArmoredKey { get; set; } = null!;
        
        public DateTime SubmissionDate { get; set; }
        
        [Required]
        public DateTime CreationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public ICollection<KeyIdentity> KeyIdentities { get; set; } = null!;
    }
}