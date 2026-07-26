//using SmartTaskManagement.Domain.Entities;

//namespace SmartTaskManagement.Domain.Interfaces;

///// <summary>
///// User-specific data access.
///// AuthService uses this instead of UserManager since we own the User entity.
///// </summary>
//public interface IUserRepository : IRepository<User, Guid>
//{
//    /// <summary>Lookup by normalized (lowercase) email. Returns null if not found.</summary>
//    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

//    /// <summary>True if any active user holds this email address.</summary>
//    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
//}


using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Domain.Interfaces;

/// <summary>
/// User-specific data access interface.
/// </summary>
public interface IUserRepository : IRepository<User, Guid>
{
    /// <summary>Lookup by email. Returns null if not found.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>True if any active user holds this email address.</summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);

    /// <summary>Searches users by email or full name with optional project member exclusion.</summary>
    Task<IReadOnlyList<User>> SearchUsersAsync(
        string keyword,
        Guid? excludeProjectId = null,
        int limit = 10,
        CancellationToken ct = default);
}