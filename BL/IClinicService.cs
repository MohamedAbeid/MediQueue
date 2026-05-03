using MediQueue.Models;

namespace MediQueue.BL
{
    public interface IClinicService
    {
        Task<IEnumerable<Clinic>> GetAllClinicsAsync();
        Task<Clinic> GetClinicByIdAsync(int clinicId);
        Task<Clinic> CreateClinicAsync(Clinic clinic);
        Task<Clinic> UpdateClinicAsync(Clinic clinic);
        Task<bool> DeleteClinicAsync(int clinicId);
        Task<bool> ClinicExistsAsync(int clinicId);
    }
}
