using System.Collections.Generic;
using MAKeys.Entities.Models;

namespace MAKeys.Contracts
{
    public interface IPublicKeyRepository : IRepositoryBase<PublicKey>
    {
        IEnumerable<PublicKey> GetAllPublicKeys();
        PublicKey GetPublicKeyByFingerprint(string fingerprint);
        void CreatePublicKey(PublicKey key);
    }
}