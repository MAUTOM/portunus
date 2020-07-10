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

        public IPublicKeyRepository PublicKey
        {
            get
            {
                if(_publicKeyRepository == null)
                    _publicKeyRepository = new PublicKeyRepository(_repoContext);

                return _publicKeyRepository;
            }
        }

        public void Save()
        {
            _repoContext.SaveChanges();
        }
    }
}