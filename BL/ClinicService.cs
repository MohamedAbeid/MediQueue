using MediQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.BL
{
    public class ClinicService : IClinicService
    {
        private readonly MediQueueContext _context;
        private readonly ILogger<ClinicService> _logger;

        public ClinicService(MediQueueContext context, ILogger<ClinicService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Clinic>> GetAllClinicsAsync()
        {
            try
            {
                return await _context.Clinics.Include(c => c.Doctors).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all clinics");
                throw;
            }
        }

        public async Task<Clinic> GetClinicByIdAsync(int clinicId)
        {
            try
            {
                return await _context.Clinics
                    .Include(c => c.Doctors)
                    .FirstOrDefaultAsync(c => c.ClinicID == clinicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving clinic with ID {clinicId}");
                throw;
            }
        }

        public async Task<Clinic> CreateClinicAsync(Clinic clinic)
        {
            try
            {
                _context.Clinics.Add(clinic);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Clinic created successfully: {clinic.Name}");
                return clinic;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating clinic");
                throw;
            }
        }

        public async Task<Clinic> UpdateClinicAsync(Clinic clinic)
        {
            try
            {
                _context.Clinics.Update(clinic);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Clinic updated successfully: {clinic.Name}");
                return clinic;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating clinic with ID {clinic.ClinicID}");
                throw;
            }
        }

        public async Task<bool> DeleteClinicAsync(int clinicId)
        {
            try
            {
                var clinic = await _context.Clinics.FindAsync(clinicId);
                if (clinic == null)
                {
                    _logger.LogWarning($"Clinic with ID {clinicId} not found");
                    return false;
                }

                _context.Clinics.Remove(clinic);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Clinic deleted successfully: {clinic.Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting clinic with ID {clinicId}");
                throw;
            }
        }

        public async Task<bool> ClinicExistsAsync(int clinicId)
        {
            try
            {
                return await _context.Clinics.AnyAsync(c => c.ClinicID == clinicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if clinic exists with ID {clinicId}");
                throw;
            }
        }
    }
}
