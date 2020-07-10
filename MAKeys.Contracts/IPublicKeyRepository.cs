using System.Collections.Generic;
using MAKeys.Entities.Models;

namespace MAKeys.Contracts
{
    public interface IPublicKeyRepository : IRepositoryBase<PublicKey>
    {
        IEnumerable<PublicKey> GetAllPublicKeys(bool trackChanges = true);
        PublicKey GetPublicKeyByFingerprint(string fingerprint, bool trackChanges = true);
        void CreatePublicKey(PublicKey key);
    }
}