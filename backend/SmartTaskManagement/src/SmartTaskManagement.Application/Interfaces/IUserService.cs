using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartTaskManagement.Application.DTOs.Users;

namespace SmartTaskManagement.Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> SearchAsync(string keyword, Guid? excludeProjectId = null, int limit = 10, CancellationToken ct = default);
    
    /// <summary>
    /// Soft delete a user. Only Admin can delete users.
    /// </summary>
    Task DeleteAsync(Guid userId, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);
}
