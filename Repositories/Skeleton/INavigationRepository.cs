using Spider_QAMS.Models.ViewModels;

namespace Spider_QAMS.Repositories.Skeleton
{
    public interface INavigationRepository
    {
        Task<string> UpdateUserVerificationAsync(UserVerifyApiVM userVerifyApiVM, int CurrentUserId);
    }
}
