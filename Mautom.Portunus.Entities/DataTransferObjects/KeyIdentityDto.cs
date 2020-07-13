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
using System.Globalization;

namespace Mautom.Portunus.Entities.DataTransferObjects
{
    public sealed class KeyIdentityDto : IFormattable
    {
        public string Name { get; set; } = null!;
        
        public string Email { get; set; } = null!;

        public string? Comment { get; set; }
        
        public DateTime CreationDate { get; set; }
        
        #region IFormattable implementation
        
        public override string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }
        
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrEmpty(format)) format = "G";
            formatProvider ??= CultureInfo.CurrentCulture;

            switch (format.ToUpperInvariant())
            {
                case "G":
                    return !string.IsNullOrEmpty(Comment) ? $"{Name} ({Comment}) <{Email}>" : $"{Name} <{Email}>";
                case "HKP":
                    return
                        $"uid:{Uri.EscapeDataString(ToString())}:{((DateTimeOffset) CreationDate).ToUnixTimeSeconds()}::";
                default:
                    throw new FormatException($"The {format} string is not implemented. Use \"G\" or \"HKP\" ");
            }
        }
        
        #endregion
    }
}