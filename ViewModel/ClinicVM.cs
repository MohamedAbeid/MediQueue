using System.ComponentModel.DataAnnotations;

namespace MediQueue.ViewModel
{
    public class ClinicVM
    {
        public int ClinicID { get; set; }

        [Required(ErrorMessage = "Clinic name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Clinic name must be between 2 and 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 200 characters.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        public string PhoneNumber { get; set; }
    }
}
