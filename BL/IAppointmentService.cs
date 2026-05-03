using MediQueue.Models;

namespace MediQueue.BL
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment> GetAppointmentByIdAsync(int appointmentId);
        Task<IEnumerable<Appointment>> GetAppointmentsByPatientAsync(string patientId);
        Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(string doctorId);
        Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date);
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<Appointment> UpdateAppointmentAsync(Appointment appointment);
        Task<bool> DeleteAppointmentAsync(int appointmentId);
        Task<bool> CancelAppointmentAsync(int appointmentId);
        Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(string userId);
        Task<bool> IsTimeSlotAvailableAsync(string doctorId, DateTime date, TimeSpan time);
    }
}
