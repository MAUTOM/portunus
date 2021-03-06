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

namespace Mautom.Portunus.Shared.Pgp
{
    [Flags]
    public enum PublicKeyFlags 
    {
        [StringValue("")]
        Default = 1,
        [StringValue("r")]
        Revoked = 2,
        [StringValue("e")]
        Expired = 4,
        [StringValue("d")]
        Disabled = 8,
        [StringValue("v")]
        Verified = 16
    }
}