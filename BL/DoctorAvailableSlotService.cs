using MediQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.BL
{
    public class DoctorAvailableSlotService : IDoctorAvailableSlotService
    {
        private readonly MediQueueContext _context;
        private readonly ILogger<DoctorAvailableSlotService> _logger;

        public DoctorAvailableSlotService(MediQueueContext context, ILogger<DoctorAvailableSlotService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<DoctorAvailableSlot>> GetAllSlotsByDoctorAsync(string doctorId)
        {
            try
            {
                return await _context.DoctorAvailableSlots
                    .Where(s => s.DoctorID == doctorId && s.IsActive)
                    .Include(s => s.Doctor)
                    .OrderBy(s => s.Date)
                    .ThenBy(s => s.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving slots for doctor {doctorId}");
                throw;
            }
        }

        public async Task<DoctorAvailableSlot> GetSlotByIdAsync(int slotId)
        {
            try
            {
                return await _context.DoctorAvailableSlots
                    .Include(s => s.Doctor)
                    .FirstOrDefaultAsync(s => s.SlotID == slotId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving slot with ID {slotId}");
                throw;
            }
        }

        public async Task<IEnumerable<DoctorAvailableSlot>> GetAvailableSlotsForDoctorAsync(string doctorId, DateTime date)
        {
            try
            {
                var startDate = date.Date;
                var endDate = date.Date.AddDays(1);

                return await _context.DoctorAvailableSlots
                    .Where(s => s.DoctorID == doctorId &&
                               s.Date >= startDate &&
                               s.Date < endDate &&
                               s.CurrentBookings < s.MaxPatients &&
                               s.IsActive)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving available slots for doctor {doctorId} on {date}");
                throw;
            }
        }

        public async Task<DoctorAvailableSlot> CreateSlotAsync(DoctorAvailableSlot slot)
        {
            try
            {
                if (slot.EndTime <= slot.StartTime)
                    throw new ArgumentException("End time must be after start time");

                // لو محتاج تستخدم totalMinutes، استخدمها فعلاً
                var totalMinutes = (slot.EndTime - slot.StartTime).TotalMinutes;
                if (totalMinutes < 15) // مثلاً الحد الأدنى 15 دقيقة
                    throw new ArgumentException("Slot duration must be at least 15 minutes");

                slot.CreatedAt = DateTime.Now;
                _context.DoctorAvailableSlots.Add(slot);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Slot created successfully for doctor {slot.DoctorID}");
                return slot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating slot");
                throw;
            }
        }

        public async Task<DoctorAvailableSlot> UpdateSlotAsync(DoctorAvailableSlot slot)
        {
            try
            {
                _context.DoctorAvailableSlots.Update(slot);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Slot updated successfully: ID {slot.SlotID}");
                return slot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating slot with ID {slot.SlotID}");
                throw;
            }
        }

        public async Task<bool> DeleteSlotAsync(int slotId)
        {
            try
            {
                var slot = await _context.DoctorAvailableSlots.FindAsync(slotId);
                if (slot == null)
                {
                    _logger.LogWarning($"Slot with ID {slotId} not found");
                    return false;
                }

                slot.IsActive = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Slot deleted successfully: ID {slotId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting slot with ID {slotId}");
                throw;
            }
        }

        public async Task<int> GetAvailableSeatsAsync(int slotId)
        {
            try
            {
                var slot = await _context.DoctorAvailableSlots.FindAsync(slotId);
                if (slot == null)
                    return 0;

                return slot.MaxPatients - slot.CurrentBookings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving available seats for slot {slotId}");
                throw;
            }
        }

        public async Task<bool> BookSlotAsync(int slotId)
        {
            try
            {
                var slot = await _context.DoctorAvailableSlots.FindAsync(slotId);
                if (slot == null)
                {
                    _logger.LogWarning($"Slot with ID {slotId} not found");
                    return false;
                }

                if (slot.CurrentBookings >= slot.MaxPatients)
                {
                    _logger.LogWarning($"Slot {slotId} is full");
                    return false;
                }

                slot.CurrentBookings++;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Slot {slotId} booked successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error booking slot {slotId}");
                throw;
            }
        }

        public async Task<IEnumerable<DoctorAvailableSlot>> GetSlotsForAppointmentAsync(string doctorId, DateTime date)
        {
            try
            {
                var startDate = date.Date;
                var endDate = date.Date.AddDays(1);

                return await _context.DoctorAvailableSlots
                    .Where(s => s.DoctorID == doctorId &&
                               s.Date >= startDate &&
                               s.Date < endDate &&
                               s.IsActive)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving slots for appointment");
                throw;
            }
        }
    }
}
