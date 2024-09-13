namespace Spider_QAMS.Models.ViewModels
{
    public class UserVerifyApiVM
    {
        public int UserId { get; set; }
        public string Designation { get; set; }
        public string FullName { get; set; }
        public string EmailID { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public string ProfilePicName { get; set; }
        public int CreateUserId { get; set; }
        public int UpdateUserId { get; set; }
    }
}
