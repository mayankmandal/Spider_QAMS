namespace Spider_QAMS.Repositories.Skeleton
{
    public interface IEmailService
    {
        Task SendAsync(string from, string to, string subject, string body);
    }
}
