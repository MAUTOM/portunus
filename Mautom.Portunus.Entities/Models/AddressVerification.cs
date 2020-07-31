using System;
using System.ComponentModel.DataAnnotations;

namespace Mautom.Portunus.Entities.Models
{
    public class AddressVerification
    {
        [Key]
        public int VerificationId { get; set; }
        
        public Guid Token { get; set; }
        
        public string Email { get; set; } = null!;
        
        public ushort VerificationCode { get; set; }
    }
}