namespace MediQueue.Models
{
    public class Clinic
    {
        public int ClinicID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public ICollection<User> Doctors { get; set; }

    }
}
