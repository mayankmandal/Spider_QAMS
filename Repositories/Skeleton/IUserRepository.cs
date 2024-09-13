using Spider_QAMS.Models;

namespace Spider_QAMS.Repositories.Skeleton
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetUserByEmailAsyncRepo(string email);
        Task<ApplicationUser> GetUserByIdAsyncRepo(int userId);
        Task<IList<string>> GetUserRolesAsyncRepo(int userId);
        Task<ApplicationUser> RegisterUserAsyncRepo(ApplicationUser user);
    }
}
