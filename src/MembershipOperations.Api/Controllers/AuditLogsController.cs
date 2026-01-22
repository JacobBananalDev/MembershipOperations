using MembershipOperations.Infrastructure.Persistence;
using MembershipOperations.Shared.Dto.Audit;
using MembershipOperations.Shared.Dto.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MembershipOperations.Api.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AuditLogsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        [FromQuery] string? actor = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 50 : pageSize;
        pageSize = Math.Min(pageSize, 200);

        var query = _db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            entityType = entityType.Trim();
            query = query.Where(x => x.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            entityId = entityId.Trim();
            query = query.Where(x => x.EntityId == entityId);
        }

        if (!string.IsNullOrWhiteSpace(actor))
        {
            actor = actor.Trim();
            query = query.Where(x => x.Actor.Contains(actor));
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            action = action.Trim();
            query = query.Where(x => x.Action == action);
        }

        if (fromUtc.HasValue)
            query = query.Where(x => x.TimestampUtc >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(x => x.TimestampUtc <= toUtc.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.TimestampUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditLogDto
            {
                Id = x.Id,
                Actor = x.Actor,
                Action = x.Action,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                TimestampUtc = x.TimestampUtc,
                DetailsJson = x.DetailsJson
            })
            .ToListAsync();

        return Ok(new PagedResult<AuditLogDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<AuditLogDto>> GetAuditLogById(long id)
    {
        var item = await _db.AuditLogs.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AuditLogDto
            {
                Id = x.Id,
                Actor = x.Actor,
                Action = x.Action,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                TimestampUtc = x.TimestampUtc,
                DetailsJson = x.DetailsJson
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound();

        return Ok(item);
    }
}
