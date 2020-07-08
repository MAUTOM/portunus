namespace MAKeys.Contracts
{
    public interface IRepositoryWrapper
    {
        IPublicKeyRepository PublicKey { get; }
        void Save();
    }
}