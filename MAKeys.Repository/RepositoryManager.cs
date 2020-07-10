using MAKeys.Contracts;
using MAKeys.Entities;

namespace MAKeys.Repository
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly RepositoryContext _repoContext;
        private IPublicKeyRepository _publicKeyRepository;
        
        public RepositoryManager(RepositoryContext context)
        {
            _repoContext = context;
        }

        public IPublicKeyRepository PublicKey => _publicKeyRepository ??= new PublicKeyRepository(_repoContext);

        public void Save() => _repoContext.SaveChanges();
    }
}