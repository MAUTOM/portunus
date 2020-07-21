using System;
using System.Collections.Generic;
using System.Linq;
using Mautom.Portunus.Contracts;
using Mautom.Portunus.Entities;
using Mautom.Portunus.Entities.Models;
using Microsoft.EntityFrameworkCore;

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
                    id.PublicKeyFingerprint.Equals(fingerprint), trackChanges)
                .OrderBy(id => id.IdentityId);

        public IEnumerable<KeyIdentity> SearchIdentities(string searchPattern, bool exact = false, bool trackChanges = true)
        {
            if(!exact)
                return FindByCondition(id => id.Name.Contains(searchPattern) 
                                             || id.Email.Contains(searchPattern) 
                                             || (id.Comment != null && id.Comment.Contains(searchPattern)), false)
                .OrderBy(id => id.Name)
                .Include(id => id.PublicKey);
            return FindByCondition(id => id.Name.Equals(searchPattern, StringComparison.InvariantCultureIgnoreCase)
                                         || id.Email.Equals(searchPattern,
                                             StringComparison.InvariantCultureIgnoreCase)
                                         || (id.Comment != null && id.Comment.Equals(searchPattern,
                                             StringComparison.InvariantCultureIgnoreCase)), trackChanges)
                .OrderBy(id => id.Name)
                .Include(id => id.PublicKey);
        }

        public KeyIdentity? GetIdentityByEmail(string fingerprint, string email, bool trackChanges = true) =>
            FindByCondition(
                    identity =>
                        identity.PublicKeyFingerprint.Equals(fingerprint) &&
                        identity.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase), trackChanges)
                .SingleOrDefault();

        public void CreateKeyIdentity(KeyIdentity key)
        {
            throw new NotImplementedException();
        }

        public KeyIdentityRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }
    }
}