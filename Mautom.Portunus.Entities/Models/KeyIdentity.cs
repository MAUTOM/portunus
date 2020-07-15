// Copyright (c) 2020 Bence Horváth <horvath.bence@mautom.hu>.
//
// This file is part of Portunus OpenPGP key server 
// (see https://www.horvathb.dev/portunus).
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mautom.Portunus.Shared.Pgp;

namespace Mautom.Portunus.Entities.Models
{
    public class KeyIdentity
    {
        [Key]
        public long IdentityId { get; set; }

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
        
        [Column(TypeName = "varchar(40)")]
        public PublicKeyFingerprint PublicKeyFingerprint { get; set; } = null!;
        
        [ForeignKey(nameof(PublicKeyFingerprint))]
        public virtual PublicKey PublicKey { get; set; } = null!;
        
    }
}