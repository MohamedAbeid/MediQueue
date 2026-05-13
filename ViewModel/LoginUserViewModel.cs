using System.ComponentModel.DataAnnotations;

namespace MediQueue.ViewModel
{
    public class LoginUserViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }
}
