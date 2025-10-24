using AutoMapper;
using HouseLedger.Services.Ancillary.Application.Contracts.ServiceUsers;
using HouseLedger.Services.Ancillary.Application.Interfaces;
using HouseLedger.Services.Ancillary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseLedger.Services.Ancillary.Application.Services;

/// <summary>
/// Query service for ServiceUser entities.
/// </summary>
public class ServiceUserQueryService : IServiceUserQueryService
{
    private readonly AncillaryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ServiceUserQueryService> _logger;

    public ServiceUserQueryService(
        AncillaryDbContext context,
        IMapper mapper,
        ILogger<ServiceUserQueryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceUserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting service user by ID: {Id}", id);

        var serviceUser = await _context.ServiceUsers
            .FirstOrDefaultAsync(su => su.Id == id && su.IsActive, cancellationToken);

        if (serviceUser == null)
        {
            _logger.LogWarning("Service user not found: {Id}", id);
            return null;
        }

        _logger.LogInformation("Service user found: {Id} - {Name} {Surname}", id, serviceUser.Name, serviceUser.Surname);
        return _mapper.Map<ServiceUserDto>(serviceUser);
    }

    public async Task<IEnumerable<ServiceUserDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all service users (includeInactive: {IncludeInactive})", includeInactive);

        var query = _context.ServiceUsers.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(su => su.IsActive);
        }

        var serviceUsers = await query
            .OrderBy(su => su.Surname)
            .ThenBy(su => su.Name)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} service users", serviceUsers.Count);
        return _mapper.Map<IEnumerable<ServiceUserDto>>(serviceUsers);
    }
}
