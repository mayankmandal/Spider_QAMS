namespace Spider_QAMS.Models.ViewModels
{
    public class SettingsAPIVM
    {
        public int SettingsUserId { get; set; }
        public string SettingsFullName { get; set; }
        public string SettingsUserName { get; set; }
        public string SettingsEmailID { get; set; }
        public string SettingsProfilePictureFile { get; set; } = string.Empty;
        public string SettingsProfilePictureName { get; set; } = string.Empty;
        public string SettingsPassword { get; set; } = string.Empty;
        public string SettingsReTypePassword { get; set; } = string.Empty;
    }
}
