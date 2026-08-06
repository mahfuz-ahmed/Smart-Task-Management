using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.DTOs.Tasks;

namespace SmartTaskManagement.Application.Interfaces.Services;

public interface ITaskService
{
    Task<PagedResult<TaskDto>> GetTasksAsync(Guid projectId, TaskQueryDto query, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);

    Task<PagedResult<TaskDto>> GetMyTasksAsync(TaskQueryDto query, Guid userId, CancellationToken ct = default);

    Task<TaskDto> GetByIdAsync(Guid projectId, Guid taskId, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);

    Task<TaskDto> CreateAsync(Guid projectId, CreateTaskDto dto, Guid createdByUserId, IEnumerable<string> roles, CancellationToken ct = default);

    Task<TaskDto> UpdateAsync(Guid projectId, Guid taskId, UpdateTaskDto dto, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);

    Task<TaskDto> UpdateStatusAsync(Guid projectId, Guid taskId, UpdateTaskStatusDto dto, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);

    Task<TaskDto> AssignAsync(Guid projectId, Guid taskId, AssignTaskDto dto, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);

    Task DeleteAsync(Guid projectId, Guid taskId, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);

    Task<IEnumerable<TaskActivityLogDto>> GetActivityLogsAsync(Guid taskId, CancellationToken ct = default);
}