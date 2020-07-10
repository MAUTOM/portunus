namespace MAKeys.Shared
{
    /// <summary>
    /// Available public key algorithms in OpenPGP as per RFC 4880.
    /// </summary>
    public enum PublicKeyAlgorithm
    {
        [StringValue("rsa")]
        RsaEs = 1,
        [StringValue("rsa")]
        RsaE = 2,
        [StringValue("rsa")]
        RsaS = 3,
        [StringValue("elgamal")]
        Elgamal = 16,
        [StringValue("dsa")]
        Dsa = 17,
        [StringValue("ecc")]
        EllipticCurve = 18,
        [StringValue("ecdsa")]
        Ecdsa = 19,
        [StringValue("diffie-hellman")]
        DiffieHellman = 21
    }
}