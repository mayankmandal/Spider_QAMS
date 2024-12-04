using Spider_QAMS.Repositories.Skeleton;
using System.Data.SqlClient;

namespace Spider_QAMS.Repositories.Domain
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly SqlConnection _connection;
        private SqlTransaction _transaction;

        public INavigationRepository NavigationRepository { get; }
        public IUserRepository UserRepository {  get; }

        public UnitOfWork(
            string connectionString,
            INavigationRepository navigationRepository,
            IUserRepository userRepository)
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();

            NavigationRepository = navigationRepository;
            UserRepository = userRepository;

            // Inject the transaction only if the repository is provided
            (NavigationRepository as NavigationRepository)?.SetTransaction(_transaction);
            (UserRepository as UserRepository)?.SetTransaction(_transaction);
        }

        public async Task<int> CommitAsync()
        {
            try
            {
                // Commit the transaction
                await _transaction.CommitAsync();
                return 1; // Success
            }
            catch
            {
                // Rollback on error
                Rollback();
                throw;
            }
        }
        public void Rollback()
        {
            // Rollback transaction
            _transaction.Rollback();
        }
        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }
    }
}
