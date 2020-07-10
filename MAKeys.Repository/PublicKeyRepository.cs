using System.Collections.Generic;
using System.Linq;
using MAKeys.Contracts;
using MAKeys.Entities;
using MAKeys.Entities.Models;

namespace MAKeys.Repository
{
    public class PublicKeyRepository : RepositoryBase<PublicKey>, IPublicKeyRepository
    {
        public PublicKeyRepository(RepositoryContext context) : base(context)
        {
            
        }

        public IEnumerable<PublicKey> GetAllPublicKeys(bool trackChanges = true)
        {
            return FindAll(trackChanges).OrderBy(pk => pk.SubmissionDate).ToList();
        }

        public PublicKey GetPublicKeyByFingerprint(string fingerprint, bool trackChanges = true)
        {
            return FindByCondition(key => key.Fingerprint.Equals(fingerprint), trackChanges).FirstOrDefault();
        }

        public void CreatePublicKey(PublicKey key)
        {
            Create(key);
        }
    }
}