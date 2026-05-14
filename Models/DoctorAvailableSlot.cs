using System.ComponentModel.DataAnnotations;

namespace MediQueue.Models
{
    public class DoctorAvailableSlot
    {
        [Key]
        public int SlotID { get; set; }

        public string DoctorID { get; set; }
        public User Doctor { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int MaxPatients { get; set; }

        public int CurrentBookings { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}
