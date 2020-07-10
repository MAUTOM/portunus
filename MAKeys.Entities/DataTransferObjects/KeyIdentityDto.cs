using System;
using System.Globalization;

namespace MAKeys.Entities.DataTransferObjects
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
                    return $"{Name} ({Comment}) <{Email}>";
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