using MediQueue.Models;

namespace MediQueue.BL
{
    public interface IQueueService
    {
        Task<IEnumerable<Queue>> GetAllQueuesAsync();
        Task<Queue> GetQueueByIdAsync(int queueId);
        Task<IEnumerable<Queue>> GetQueuesByDoctorAsync(string doctorId);
        Task<Queue> GetQueueByAppointmentAsync(int appointmentId);
        Task<Queue> CreateQueueAsync(Queue queue);
        Task<Queue> UpdateQueueAsync(Queue queue);
        Task<bool> DeleteQueueAsync(int queueId);
        Task<bool> DeactivateQueueAsync(int queueId);
        Task<IEnumerable<Queue>> GetActiveQueuesAsync(string doctorId);
        Task<int> GetNextPositionAsync(string doctorId);
        Task ReorderQueueAsync(string doctorId);
    }
}
