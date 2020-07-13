using System.Collections.Generic;
using Mautom.Portunus.Entities.Models;

namespace Mautom.Portunus.Contracts
{
    public interface IKeyIdentityRepository : IRepositoryBase<KeyIdentity>
    {
        IEnumerable<KeyIdentity> GetAllKeyIdentities(bool trackChanges = true);
        IEnumerable<KeyIdentity> GetIdentitiesByFingerprint(string fingerprint, bool trackChanges = true);
        void CreateKeyIdentity(KeyIdentity key);
    }
}