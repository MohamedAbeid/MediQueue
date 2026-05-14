using System.ComponentModel.DataAnnotations;

namespace MediQueue.ViewModel
{
    public class ResetPasswordViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور الجديدة يجب أن تتكون من 6 أحرف على الأقل.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور الجديدة وتأكيد كلمة المرور غير متطابقين.")]
        public string ConfirmNewPassword { get; set; }
    }
}
