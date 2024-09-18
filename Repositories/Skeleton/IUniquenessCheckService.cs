namespace Spider_QAMS.Repositories.Skeleton
{
    public interface IUniquenessCheckService
    {
        Task<bool> IsUniqueAsync(string field, string value);
    }
}
