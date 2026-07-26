using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Domain.Interfaces;

public interface ITaskCommentRepository : IRepository<TaskComment, Guid>
{
    Task<IEnumerable<TaskComment>> GetByTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
}
