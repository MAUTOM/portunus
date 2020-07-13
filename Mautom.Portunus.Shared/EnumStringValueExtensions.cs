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

namespace Mautom.Portunus.Shared
{
    public static class EnumStringValueExtensions
    {
        /// <summary>
        /// Will get the string value for a given enums value, this will
        /// only work if you assign the StringValue attribute to
        /// the items in your enum.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The specified string value or an empty string if it is not defined.</returns>
        public static string GetStringValue(this Enum value) {
            // Get the type
            var type = value.GetType();

            // Get fieldinfo for this type
            var fieldInfo = type.GetField(value.ToString());
            
            // Return the first if there was a match.
            return fieldInfo?.GetCustomAttributes(
                typeof(StringValueAttribute), false) is StringValueAttribute[] attribs && attribs.Length > 0 ? attribs[0].StringValue : string.Empty;
        }
    }
}