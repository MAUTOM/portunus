using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAKeys.Entities.Models
{
    [Table("KeyIdentities")]
    public sealed class KeyIdentity
    {
        [Column("IdentityId")]
        [Key]
        public long Id { get; set; }

        [Required(ErrorMessage = "Name is required for identity!")]
        [Display(Name = "Real name used for this identity.")]
        [MaxLength(50, ErrorMessage = "Real name of identity must not exceed 50 characters in length.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "E-mail address is required for identity!")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "E-mail address")]
        [EmailAddress(ErrorMessage = "Invalid e-mail address")]
        public string Email { get; set; } = null!;
        
        [Display(Name = "Comment specified for User ID.")]
        [MaxLength(200, ErrorMessage = "User ID comment must not exceed 200 characters.")]
        public string? Comment { get; set; }
        
        [Required(ErrorMessage = "User ID creation date must be specified.")]
        public DateTime CreationDate { get; set; }

        [ForeignKey(nameof(PublicKey))] 
        public string PublicKeyFingerprint { get; set; } = null!;
        public PublicKey PublicKey { get; set; } = null!;

        public override string ToString()
        {
            return $"{Name} ({Comment}) <{Email}>";
        }
    }
}