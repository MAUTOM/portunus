using System.Collections.Generic;
using System.Linq;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities;
using Mautom.Portunus.Entities.Models;

namespace Mautom.Portunus.Repository
{
    public class KeyIdentityRepository : RepositoryBase<KeyIdentity>, IKeyIdentityRepository
    {
        public IEnumerable<KeyIdentity> GetAllKeyIdentities(bool trackChanges = true)
        {
            return FindAll(trackChanges).OrderBy(identity => identity.IdentityId).ToList();
        }

        public IEnumerable<KeyIdentity> GetIdentitiesByFingerprint(string fingerprint, bool trackChanges = true)
        {
            throw new System.NotImplementedException();
        }

        public void CreateKeyIdentity(KeyIdentity key)
        {
            throw new System.NotImplementedException();
        }

        public KeyIdentityRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }
    }
}