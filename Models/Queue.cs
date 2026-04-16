namespace MediQueue.Models
{
    public class Queue
    {
        public int QueueID { get; set; }

        public int AppointmentID { get; set; }

        public Appointment Appointment { get; set; }

        public  string DoctorID { get; set; }
        public User Doctor { get; set; }
        public int Position { get; set; }
        public DateTime EstimatedTime { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
