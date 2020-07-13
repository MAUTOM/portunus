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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mautom.Portunus.Shared;

namespace Mautom.Portunus.Entities.Models
{
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

        public List<KeyIdentity> KeyIdentities { get; set; } = null!;
    }
}