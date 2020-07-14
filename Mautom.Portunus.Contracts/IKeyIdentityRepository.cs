using System.Collections.Generic;
using Mautom.Portunus.Entities.Models;

namespace Mautom.Portunus.Contracts
{
    public interface IKeyIdentityRepository : IRepositoryBase<KeyIdentity>
    {
        IEnumerable<KeyIdentity> GetAllKeyIdentities(bool trackChanges = true);
        IEnumerable<KeyIdentity> GetIdentities(string fingerprint, bool trackChanges = true);
        KeyIdentity GetIdentityByEmail(string fingerprint, string email, bool trackChanges = true);
        void CreateKeyIdentity(KeyIdentity key);
    }
}