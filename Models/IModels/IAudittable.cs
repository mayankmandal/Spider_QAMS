namespace Spider_QAMS.Models.IModels
{
    public interface IAudittable
    {
        DateTime CreateDate { get; set; }
        int CreateUserId { get; set; }
        DateTime UpdateDate { get; set; }
        int UpdateUserId { get; set; }
    }
}
