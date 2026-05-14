using MediQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.BL
{
    public class AppointmentService : IAppointmentService
    {
        private readonly MediQueueContext _context;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(MediQueueContext context, ILogger<AppointmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
        {
            try
            {
                return await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Include(a => a.Queue)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all appointments");
                throw;
            }
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int appointmentId)
        {
            try
            {
                return await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Include(a => a.Queue)
                    .FirstOrDefaultAsync(a => a.AppointmentID == appointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving appointment with ID {appointmentId}");
                throw;
            }
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(string patientId)
        {
            try
            {
                return await _context.Appointments
                    .Where(a => a.PatientID == patientId)
                    .Include(a => a.Doctor)
                    .Include(a => a.Queue)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving appointments for patient {patientId}");
                throw;
            }
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(string doctorId)
        {
            try
            {
                return await _context.Appointments
                    .Where(a => a.DoctorID == doctorId)
                    .Include(a => a.Patient)
                    .Include(a => a.Queue)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving appointments for doctor {doctorId}");
                throw;
            }
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date)
        {
            try
            {
                var startDate = date.Date;
                var endDate = date.Date.AddDays(1);

                return await _context.Appointments
                    .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate < endDate)
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Include(a => a.Queue)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving appointments for date {date}");
                throw;
            }
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            try
            {
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Appointment created successfully: ID {appointment.AppointmentID}");
                return appointment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                throw;
            }
        }

        public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)
        {
            try
            {
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Appointment updated successfully: ID {appointment.AppointmentID}");
                return appointment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating appointment with ID {appointment.AppointmentID}");
                throw;
            }
        }

        public async Task<bool> DeleteAppointmentAsync(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning($"Appointment with ID {appointmentId} not found");
                    return false;
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Appointment deleted successfully: ID {appointmentId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting appointment with ID {appointmentId}");
                throw;
            }
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning($"Appointment with ID {appointmentId} not found");
                    return false;
                }

                appointment.Status = AppointmentStatus.Cancelled;

                // Release the slot
                var slot = await _context.DoctorAvailableSlots
                    .FirstOrDefaultAsync(s => s.DoctorID == appointment.DoctorID &&
                                              s.Date.Date == appointment.AppointmentDate.Date &&
                                              s.StartTime <= appointment.AppointmentTime &&
                                              s.EndTime > appointment.AppointmentTime &&
                                              s.IsActive);
                
                if (slot != null && slot.CurrentBookings > 0)
                {
                    slot.CurrentBookings--;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Appointment cancelled successfully: ID {appointmentId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling appointment with ID {appointmentId}");
                throw;
            }
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(string userId)
        {
            try
            {
                var now = DateTime.Now;
                return await _context.Appointments
                    .Where(a => (a.PatientID == userId || a.DoctorID == userId) && 
                                a.AppointmentDate >= now &&
                                a.Status != AppointmentStatus.Cancelled)
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving upcoming appointments for user {userId}");
                throw;
            }
        }

        public async Task<bool> IsTimeSlotAvailableAsync(string doctorId, DateTime date, TimeSpan time)
        {
            try
            {
                var startDate = date.Date;
                var endDate = date.Date.AddDays(1);

                var existingAppointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.DoctorID == doctorId &&
                                             a.AppointmentDate >= startDate &&
                                             a.AppointmentDate < endDate &&
                                             a.AppointmentTime == time &&
                                             a.Status != AppointmentStatus.Cancelled);

                return existingAppointment == null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking time slot availability for doctor {doctorId}");
                throw;
            }
        }
    }
}
