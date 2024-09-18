using Spider_QAMS.Models.IModels;

namespace Spider_QAMS.Models
{
    public class Appointment :IAudittable
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public int Duration { get; set; }
        public int LocationId { get; set; }
        public int AssignedToUserId { get; set; }
        public string Comments { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateUserId { get; set; }
        public DateTime UpdateDate { get; set; }
        public int UpdateUserId { get; set; }

    }
}
