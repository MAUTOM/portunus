using System;
using System.Globalization;
using System.Linq;

namespace Mautom.Portunus.Shared.Pgp
{
    public sealed class PublicKeyFingerprint : IFormattable, IEquatable<PublicKeyFingerprint>
    {
        private string _fingerprint = string.Empty;
        
        public string Fingerprint
        {
            get => _fingerprint.ToUpper();
            private set
            {
                ValidateFingerprint(value);
                _fingerprint = value.ToUpper();
            }
        }

        public string LongKeyId => Fingerprint.Substring(24, 16);
        public string ShortKeyId => Fingerprint.Substring(32, 8);
        
        // Needed for EntityFramework
        public PublicKeyFingerprint()
        {
            _fingerprint = string.Empty;
        }
        
        public PublicKeyFingerprint(string fingerprint)
        {
            Fingerprint = fingerprint;
        }
        
        public static implicit operator PublicKeyFingerprint(string fingerprint)
        {
            return new PublicKeyFingerprint(fingerprint);
        }

        public static implicit operator string(PublicKeyFingerprint fingerprint) => fingerprint.Fingerprint;

        private static void ValidateFingerprint(string fingerprint)
        {
            //if (string.IsNullOrEmpty(fingerprint)) return; // for EFCore
            if(fingerprint.Length != 40) throw new PgpException("The public key fingerprint must be 160 bits long (v4).");
        }

        public static bool IsValidFingerprint(string fingerprint)
        {
            if (fingerprint.Length != 40) return false;
            
            var isAscii = fingerprint.All(c => c <= 127);

            return isAscii;
        }

        public override string ToString()
        {
            return ToString("G", CultureInfo.InvariantCulture);
        }

        public string ToString(string format)
        {
            return ToString(format, CultureInfo.InvariantCulture);
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            if (string.IsNullOrEmpty(format)) format = "G";

            return format switch
            {
                "G" => Fingerprint,
                "FP" => string.Format("{0} {1} {2} {3} {4} {5} {6}", Fingerprint.Substring(0, 4),
                    Fingerprint.Substring(4, 4), Fingerprint.Substring(8, 4), Fingerprint.Substring(16, 4),
                    Fingerprint.Substring(24, 4), Fingerprint.Substring(32, 4), Fingerprint.Substring(36, 4)),
                "X" => $"0x{Fingerprint}",
                _ => throw new FormatException($"The {format} string is not implemented. Use \"G\" or \"FP\" ")
            };
        }

        public bool Equals(PublicKeyFingerprint? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Fingerprint, other.Fingerprint, StringComparison.OrdinalIgnoreCase);
        }
        
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is PublicKeyFingerprint other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Fingerprint);
        }

        public static bool operator ==(PublicKeyFingerprint? left, PublicKeyFingerprint? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PublicKeyFingerprint? left, PublicKeyFingerprint? right)
        {
            return !Equals(left, right);
        }
    }
}