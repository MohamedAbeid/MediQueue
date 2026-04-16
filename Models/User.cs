using Microsoft.AspNetCore.Identity;

namespace MediQueue.Models
{
    public class User: IdentityUser
    {
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string? Specialty { get; set; }
        public int? ClinicID { get; set; }

        public Clinic? Clinic { get; set; }

        public ICollection<Appointment> PatientAppointments { get; set; }
        public ICollection<Appointment> DoctorAppointments { get; set; }


    }
}
