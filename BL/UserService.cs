using MediQueue.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.BL
{
    public class UserService : IUserService
    {
        private readonly MediQueueContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly UserManager<User> _userManager;

        public UserService(MediQueueContext context, ILogger<UserService> logger, UserManager<User> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                return await _context.Users.Include(u => u.Clinic).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                throw;
            }
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Clinic)
                    .Include(u => u.AvailableSlots)
                    .FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user with ID {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetDoctorsByClinicAsync(int clinicId)
        {
            try
            {
                return await _context.Users
                    .Where(u => u.ClinicID == clinicId && !string.IsNullOrEmpty(u.Specialty))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving doctors for clinic {clinicId}");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllDoctorsAsync()
        {
            try
            {
                return await _context.Users
                    .Where(u => !string.IsNullOrEmpty(u.Specialty))
                    .Include(u => u.Clinic)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all doctors");
                throw;
            }
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            try
            {
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                _logger.LogInformation($"User updated successfully: {user.FullName}");
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID {user.Id}");
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found");
                    return false;
                }

                // 1. حذف السجلات المرتبطة في الطابور (Queues)
                var doctorQueues = _context.Queues.Where(q => q.DoctorID == userId);
                if (doctorQueues.Any())
                {
                    _context.Queues.RemoveRange(doctorQueues);
                }

                // 2. حذف المواعيد (Appointments) المرتبطة بالمستخدم (سواء كان مريضاً أو طبيباً)
                var userAppointments = _context.Appointments.Where(a => a.PatientID == userId || a.DoctorID == userId);
                if (userAppointments.Any())
                {
                    _context.Appointments.RemoveRange(userAppointments);
                }

                // حفظ التغييرات لحذف البيانات المرتبطة أولاً
                await _context.SaveChangesAsync();

                // 3. حذف المستخدم
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                _logger.LogInformation($"User deleted successfully: {user.FullName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID {userId}");
                throw;
            }
        }

        public async Task<bool> UserExistsAsync(string userId)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user exists with ID {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<User>> SearchDoctorsBySpecialtyAsync(string specialty)
        {
            try
            {
                return await _context.Users
                    .Where(u => !string.IsNullOrEmpty(u.Specialty) && u.Specialty.Contains(specialty))
                    .Include(u => u.Clinic)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching doctors by specialty: {specialty}");
                throw;
            }
        }

        public async Task<(bool Success, string Message, User Doctor)> CreateDoctorAsync(User doctor, string password)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(doctor.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Doctor with email {doctor.Email} already exists");
                    return (false, "Doctor with this email already exists.", null);
                }

                doctor.UserName = doctor.Email;
                var result = await _userManager.CreateAsync(doctor, password);

                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(doctor, "Doctor");
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogError($"Failed to assign Doctor role to user {doctor.Id}");
                        return (false, "Doctor created but failed to assign role.", doctor);
                    }

                    _logger.LogInformation($"Doctor created successfully: {doctor.FullName}");
                    return (true, "Doctor created successfully.", doctor);
                }
                else
                {
                    var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError($"Failed to create doctor: {errorMessage}");
                    return (false, errorMessage, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating doctor");
                return (false, "An error occurred while creating the doctor.", null);
            }
        }

        public async Task<(bool Success, string Message)> ResetUserPasswordAsync(string userId, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return (false, "User not found.");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Password reset successfully for user: {user.FullName}");
                    return (true, "تم إعادة تعيين كلمة المرور بنجاح.");
                }
                
                var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errorMsg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting password for user {userId}");
                return (false, "حدث خطأ غير متوقع أثناء محاولة تغيير كلمة المرور.");
            }
        }
    }
}
