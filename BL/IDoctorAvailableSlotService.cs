using MediQueue.Models;

namespace MediQueue.BL
{
    public interface IDoctorAvailableSlotService
    {
        Task<IEnumerable<DoctorAvailableSlot>> GetAllSlotsByDoctorAsync(string doctorId);
        Task<DoctorAvailableSlot> GetSlotByIdAsync(int slotId);
        Task<IEnumerable<DoctorAvailableSlot>> GetAvailableSlotsForDoctorAsync(string doctorId, DateTime date);
        Task<DoctorAvailableSlot> CreateSlotAsync(DoctorAvailableSlot slot);
        Task<DoctorAvailableSlot> UpdateSlotAsync(DoctorAvailableSlot slot);
        Task<bool> DeleteSlotAsync(int slotId);
        Task<int> GetAvailableSeatsAsync(int slotId);
        Task<bool> BookSlotAsync(int slotId);
        Task<IEnumerable<DoctorAvailableSlot>> GetSlotsForAppointmentAsync(string doctorId, DateTime date);
    }
}
