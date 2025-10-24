using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Contracts.ServiceUsers;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Ancillary.Domain.Entities;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.Application.Services;

/// <summary>
/// Command service implementation for ServiceUser entity CRUD operations.
/// </summary>
public class ServiceUserCommandService : IServiceUserCommandService
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ServiceUserCommandService> _logger;

    public ServiceUserCommandService(
        AncillaryDbContext context,
        IMapper mapper,
        ILogger<ServiceUserCommandService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceUserDto> CreateAsync(CreateServiceUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new service user: {Name} {Surname}", request.Name, request.Surname);

        var serviceUser = _mapper.Map<ServiceUser>(request);
        serviceUser.CreatedDate = DateTime.UtcNow;
        serviceUser.LastUpdatedDate = DateTime.UtcNow;
        serviceUser.IsActive = true;

        _context.ServiceUsers.Add(serviceUser);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Service user created successfully with ID: {Id}", serviceUser.Id);

        return _mapper.Map<ServiceUserDto>(serviceUser);
    }

    public async Task<ServiceUserDto?> UpdateAsync(int id, UpdateServiceUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating service user with ID: {Id}", id);

        var serviceUser = await _context.ServiceUsers
            .FirstOrDefaultAsync(su => su.Id == id, cancellationToken);

        if (serviceUser == null)
        {
            _logger.LogWarning("Service user with ID {Id} not found", id);
            return null;
        }

        _mapper.Map(request, serviceUser);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Service user with ID {Id} updated successfully", id);

        return _mapper.Map<ServiceUserDto>(serviceUser);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Soft deleting service user with ID: {Id}", id);

        var serviceUser = await _context.ServiceUsers
            .FirstOrDefaultAsync(su => su.Id == id, cancellationToken);

        if (serviceUser == null)
        {
            _logger.LogWarning("Service user with ID {Id} not found", id);
            return false;
        }

        serviceUser.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Service user with ID {Id} soft deleted successfully", id);

        return true;
    }

    public async Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Hard deleting service user with ID: {Id}", id);

        var serviceUser = await _context.ServiceUsers
            .FirstOrDefaultAsync(su => su.Id == id, cancellationToken);

        if (serviceUser == null)
        {
            _logger.LogWarning("Service user with ID {Id} not found", id);
            return false;
        }

        _context.ServiceUsers.Remove(serviceUser);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Service user with ID {Id} hard deleted permanently", id);

        return true;
    }
}
