namespace MAKeys.Contracts
{
    public interface IRepositoryManager
    {
        IPublicKeyRepository PublicKey { get; }
        void Save();
    }
}