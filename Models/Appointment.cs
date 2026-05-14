namespace MediQueue.Models
{
    public class Appointment
    {
        public int AppointmentID { get; set; }

        public string PatientID { get; set; }
        public User Patient { get; set; }

        public string DoctorID { get; set; }
        public User Doctor { get; set; }

        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;

        public Queue Queue { get; set; }

       
    }
}
