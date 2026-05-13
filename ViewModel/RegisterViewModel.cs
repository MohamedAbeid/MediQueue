using System.ComponentModel.DataAnnotations;

namespace MediQueue.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        [MinLength(2)]
        public string FullName { get; set; }
        [Required]
        [MinLength(2)]
        [MaxLength(30)]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]

        public string Email { get; set; }
        [Required]
        [MinLength(2)]
        public string PhoneNumber { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]

        public string PasswordConfirm { get; set; }
    }
}
