using System.ComponentModel.DataAnnotations;

namespace MediQueue.ViewModel
{
    public class UserVM
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        public string PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Specialty cannot exceed 100 characters.")]
        public string? Specialty { get; set; }

        public int? ClinicID { get; set; }

        public string? UserName { get; set; }
    }
}
