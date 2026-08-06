using Microsoft.Extensions.Logging;
using SmartTaskManagement.Application.DTOs.Users;
using SmartTaskManagement.Application.Exceptions;
using SmartTaskManagement.Application.Interfaces.Services;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Interfaces;

namespace SmartTaskManagement.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork uow,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _uow = uow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserDto>> SearchAsync(
        string keyword,
        Guid? excludeProjectId = null,
        int limit = 10,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Trim().Length < 2)
            return Array.Empty<UserDto>();

        var users = await _userRepository.SearchUsersAsync(keyword, excludeProjectId, limit, ct);

        return users
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                $"{u.FirstName} {u.LastName}".Trim(),
                u.Role))
            .ToList();
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid requestingUserId,
        IEnumerable<string> roles,
        CancellationToken ct = default)
    {
        // Only Admin can delete users
        if (!AuthorizationHelper.IsAdmin(roles))
            throw new ForbiddenException("Only Admins can delete users.");

        var user = await _uow.Users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException(nameof(User), userId);

        // Prevent self-deletion
        if (userId == requestingUserId)
            throw new BusinessException("You cannot delete your own account.");

        // Soft delete user
        user.IsDeleted = true;
        user.DeletedAtUtc = DateTime.UtcNow;
        user.DeletedBy = requestingUserId.ToString();
        user.IsActive = false;

        _uow.Users.Update(user);

        // Deactivate all project memberships
        var memberships = await _uow.ProjectMembers.FindAsync(pm => pm.UserId == userId, ct);
        foreach (var membership in memberships.Where(m => m.IsActive))
        {
            membership.IsActive = false;
        }

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("User soft-deleted: {UserId} by Admin {RequestingUserId}", userId, requestingUserId);
    }
}