using System.ComponentModel.DataAnnotations;
using MediQueue.Models;

namespace MediQueue.ViewModel
{
    public class AppointmentVM
    {
        public int AppointmentID { get; set; }

        [Required(ErrorMessage = "Patient is required.")]
        public string PatientID { get; set; }

        public string PatientName { get; set; }

        [Required(ErrorMessage = "Doctor is required.")]
        public string DoctorID { get; set; }

        public string DoctorName { get; set; }

        [Required(ErrorMessage = "Appointment date is required.")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Appointment time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    }
}
