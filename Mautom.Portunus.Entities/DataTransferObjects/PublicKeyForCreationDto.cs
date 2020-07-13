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
using Mautom.Portunus.Shared;

namespace Mautom.Portunus.Entities.DataTransferObjects
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