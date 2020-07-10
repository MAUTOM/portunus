using System;
using System.Reflection;

namespace MAKeys.Shared
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