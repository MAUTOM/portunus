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
using Mautom.Portunus.Shared;

namespace Mautom.Portunus.Entities.DataTransferObjects
{
    public sealed class PublicKeyDto
    {
        public string Fingerprint { get; set; } = null!;

        public ICollection<KeyIdentityDto> KeyIdentities { get; set; } = null!;

        public string ArmoredKey { get; set; } = null!;

        public PublicKeyAlgorithm Algorithm { get; set; }

        public PublicKeyLength Length { get; set; }

        public PublicKeyFlags Flags { get; set; }

        public DateTime SubmissionDate { get; set; }
        
        public DateTime CreationDate { get; set; }
        
        public DateTime? ExpirationDate { get; set; }
    }
}