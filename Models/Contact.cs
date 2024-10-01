namespace Spider_QAMS.Models
{
    public class Contact
    {
        public int ContactId { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string OfficePhone { get; set; }
        public string Mobile { get; set; }
        public string EmailID { get; set; }
        public string Fax { get; set; }
        public string BranchName { get; set; }
        public Sponsor sponsor { get; set; }

    }
}
