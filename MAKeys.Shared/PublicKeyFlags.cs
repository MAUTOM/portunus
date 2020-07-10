using System;

namespace MAKeys.Shared
{
    [Flags]
    public enum PublicKeyFlags 
    {
        [StringValue("")]
        None = 0,
        [StringValue("r")]
        Revoked = 1,
        [StringValue("e")]
        Expired = 2,
        [StringValue("d")]
        Disabled = 4
    }
}