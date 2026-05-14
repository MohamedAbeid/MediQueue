using System.ComponentModel.DataAnnotations;

namespace MediQueue.ViewModel
{
    public class AvailableSlotViewModel
    {
        public int SlotID { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Maximum patients is required.")]
        [Range(1, 50, ErrorMessage = "Maximum patients must be between 1 and 50.")]
        public int MaxPatients { get; set; }

        public string DoctorID { get; set; }
        public string DoctorName { get; set; }

        public int AvailableSlotsCount { get; set; }
    }
}
