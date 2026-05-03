using System.ComponentModel.DataAnnotations;

namespace MediQueue.ViewModel
{
    public class QueueVM
    {
        public int QueueID { get; set; }

        [Required(ErrorMessage = "Appointment is required.")]
        public int AppointmentID { get; set; }

        public string AppointmentDetails { get; set; }

        [Required(ErrorMessage = "Doctor is required.")]
        public string DoctorID { get; set; }

        public string DoctorName { get; set; }

        [Required(ErrorMessage = "Position is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Position must be greater than 0.")]
        public int Position { get; set; }

        [Required(ErrorMessage = "Estimated time is required.")]
        [DataType(DataType.DateTime)]
        public DateTime EstimatedTime { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
