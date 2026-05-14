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
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
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
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found");
                    return false;
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
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
    }
}
