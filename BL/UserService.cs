using MediQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.BL
{
    public class UserService : IUserService
    {
        private readonly MediQueueContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(MediQueueContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
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
    }
}
