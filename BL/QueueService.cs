using MediQueue.Models;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.BL
{
    public class QueueService : IQueueService
    {
        private readonly MediQueueContext _context;
        private readonly ILogger<QueueService> _logger;

        public QueueService(MediQueueContext context, ILogger<QueueService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Queue>> GetAllQueuesAsync()
        {
            try
            {
                return await _context.Queues
                    .Include(q => q.Appointment)
                    .Include(q => q.Doctor)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all queues");
                throw;
            }
        }

        public async Task<Queue> GetQueueByIdAsync(int queueId)
        {
            try
            {
                return await _context.Queues
                    .Include(q => q.Appointment)
                    .Include(q => q.Doctor)
                    .FirstOrDefaultAsync(q => q.QueueID == queueId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving queue with ID {queueId}");
                throw;
            }
        }

        public async Task<IEnumerable<Queue>> GetQueuesByDoctorAsync(string doctorId)
        {
            try
            {
                return await _context.Queues
                    .Where(q => q.DoctorID == doctorId)
                    .Include(q => q.Appointment)
                    .OrderBy(q => q.Position)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving queues for doctor {doctorId}");
                throw;
            }
        }

        public async Task<Queue> GetQueueByAppointmentAsync(int appointmentId)
        {
            try
            {
                return await _context.Queues
                    .Include(q => q.Appointment)
                    .Include(q => q.Doctor)
                    .FirstOrDefaultAsync(q => q.AppointmentID == appointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving queue for appointment {appointmentId}");
                throw;
            }
        }

        public async Task<Queue> CreateQueueAsync(Queue queue)
        {
            try
            {
                _context.Queues.Add(queue);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Queue created successfully: ID {queue.QueueID}");
                return queue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating queue");
                throw;
            }
        }

        public async Task<Queue> UpdateQueueAsync(Queue queue)
        {
            try
            {
                _context.Queues.Update(queue);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Queue updated successfully: ID {queue.QueueID}");
                return queue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating queue with ID {queue.QueueID}");
                throw;
            }
        }

        public async Task<bool> DeleteQueueAsync(int queueId)
        {
            try
            {
                var queue = await _context.Queues.FindAsync(queueId);
                if (queue == null)
                {
                    _logger.LogWarning($"Queue with ID {queueId} not found");
                    return false;
                }

                _context.Queues.Remove(queue);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Queue deleted successfully: ID {queueId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting queue with ID {queueId}");
                throw;
            }
        }

        public async Task<bool> DeactivateQueueAsync(int queueId)
        {
            try
            {
                var queue = await _context.Queues.FindAsync(queueId);
                if (queue == null)
                {
                    _logger.LogWarning($"Queue with ID {queueId} not found");
                    return false;
                }

                queue.IsActive = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Queue deactivated successfully: ID {queueId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating queue with ID {queueId}");
                throw;
            }
        }

        public async Task<IEnumerable<Queue>> GetActiveQueuesAsync(string doctorId)
        {
            try
            {
                return await _context.Queues
                    .Where(q => q.DoctorID == doctorId && q.IsActive)
                    .Include(q => q.Appointment)
                    .OrderBy(q => q.Position)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving active queues for doctor {doctorId}");
                throw;
            }
        }

        public async Task<int> GetNextPositionAsync(string doctorId)
        {
            try
            {
                var maxPosition = await _context.Queues
                    .Where(q => q.DoctorID == doctorId && q.IsActive)
                    .MaxAsync(q => (int?)q.Position) ?? 0;

                return maxPosition + 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting next position for doctor {doctorId}");
                throw;
            }
        }

        public async Task ReorderQueueAsync(string doctorId)
        {
            try
            {
                var queues = await _context.Queues
                    .Where(q => q.DoctorID == doctorId && q.IsActive)
                    .OrderBy(q => q.Position)
                    .ToListAsync();

                for (int i = 0; i < queues.Count; i++)
                {
                    queues[i].Position = i + 1;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Queue reordered successfully for doctor {doctorId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reordering queue for doctor {doctorId}");
                throw;
            }
        }
    }
}
