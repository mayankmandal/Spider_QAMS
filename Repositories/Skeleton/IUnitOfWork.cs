namespace Spider_QAMS.Repositories.Skeleton
{
    public interface IUnitOfWork : IDisposable
    {
        INavigationRepository NavigationRepository { get; }
        IUserRepository UserRepository { get; }
        Task<int> CommitAsync();
        void Rollback();
    }
}
