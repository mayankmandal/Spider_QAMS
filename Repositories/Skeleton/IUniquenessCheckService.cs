namespace Spider_QAMS.Repositories.Skeleton
{
    public interface IUniquenessCheckService
    {
        Task<bool> IsUniqueAsync(string field1, string value1,string field2, string value2);
    }
}
