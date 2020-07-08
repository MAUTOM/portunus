using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAKeys.Entities.Models
{
    public class PublicKey
    {
        public long Id { get; set; }
        [Required(ErrorMessage = "Name for public key is required!")]
        [StringLength(50, ErrorMessage = "Name must not exceed 50 characters.")]
        public string Name { get; set; } = null!;
        [Required(ErrorMessage = "E-mail for public key is required!")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "No public key data")]
        public string Armor { get; set; } = null!;
        public string Fingerprint { get; set; } = null!;
        public DateTime SubmissionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}