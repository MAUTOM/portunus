using System;
using System.Collections.Generic;
using Mautom.Portunus.Entities.Models;

namespace Mautom.Portunus.Contracts
{
    public interface IKeyIdentityRepository : IRepositoryBase<KeyIdentity>
    {
        IEnumerable<KeyIdentity> GetAllKeyIdentities(bool trackChanges = true);
        IEnumerable<KeyIdentity> GetIdentities(string fingerprint, bool trackChanges = true);
        IEnumerable<KeyIdentity> SearchIdentities(string searchPattern, bool exact = false, bool trackChanges = true);
        IEnumerable<KeyIdentity> GetIdentitiesByToken(Guid token, bool trackChanges = true);
        KeyIdentity? GetIdentityByEmail(string fingerprint, string email, bool trackChanges = true);
        KeyIdentity? GetIdentityByEmail(string email, bool trackChanges = true);
        void CreateKeyIdentity(KeyIdentity key);
    }
}