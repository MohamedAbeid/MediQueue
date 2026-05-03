using MediQueue.Models;

namespace MediQueue.BL
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(string userId);
        Task<IEnumerable<User>> GetDoctorsByClinicAsync(int clinicId);
        Task<IEnumerable<User>> GetAllDoctorsAsync();
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> UserExistsAsync(string userId);
        Task<IEnumerable<User>> SearchDoctorsBySpecialtyAsync(string specialty);
    }
}
