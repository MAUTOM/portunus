using System;
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

        public IEnumerable<KeyIdentity> GetIdentities(string fingerprint, bool trackChanges = true) =>
            FindByCondition(id =>
                    id.PublicKeyFingerprint.Equals(fingerprint, StringComparison.InvariantCultureIgnoreCase), trackChanges)
                .OrderBy(id => id.IdentityId);

        public KeyIdentity GetIdentityByEmail(string fingerprint, string email, bool trackChanges = true) =>
            FindByCondition(
                    identity =>
                        identity.PublicKeyFingerprint.Equals(fingerprint,
                            StringComparison.InvariantCultureIgnoreCase) &&
                        identity.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase), trackChanges)
                .SingleOrDefault();

        public void CreateKeyIdentity(KeyIdentity key)
        {
            throw new System.NotImplementedException();
        }

        public KeyIdentityRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }
    }
}